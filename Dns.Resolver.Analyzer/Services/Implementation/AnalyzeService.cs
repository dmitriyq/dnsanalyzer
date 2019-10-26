using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTools;

namespace Dns.Resolver.Analyzer.Services.Implementation
{
	public class AnalyzeService : IAnalyzeService
	{
		private readonly ILogger<AnalyzeService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly ICacheService _cacheService;
		private readonly TimeSpan _expireInterval;
		private readonly TimeSpan _closingInterval;
		private readonly TimeSpan _falsePositiveInterval;

		public AnalyzeService(IServiceProvider serviceProvider,
			ILogger<AnalyzeService> logger, ICacheService cacheService,
			TimeSpan expireInterval, TimeSpan closingInterval, TimeSpan falsePositiveInterval)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_cacheService = cacheService;
			_expireInterval = expireInterval;
			_closingInterval = closingInterval;
			_falsePositiveInterval = falsePositiveInterval;
		}

		public AttackFoundMessage[] Exclude(AttackFoundMessage[] attacks)
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			var excludedDomains = db.DomainExcludedNames.ToList();
			return attacks
				.Where(x => !excludedDomains.Any(z => z.BlackDomain == x.BlackDomain && z.WhiteDomain == x.WhiteDomain))
				.ToArray();
		}

		public async Task<IEnumerable<int>> UpdateAttackAsync(AttackFoundMessage[] messages)
		{
			var notifyAttacks = new List<int>();

			var vigruzkiIps = await _cacheService.GetVigruzkiIps().ConfigureAwait(true);
			var vigruzkiSubnets = await _cacheService.GetVigruzkiSubnets().ConfigureAwait(true);

			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			foreach (var message in messages)
			{
				var ipBlocked = vigruzkiIps.Contains(message.Ip);
				string? subnetBlocked = default;
				try
				{
					subnetBlocked = vigruzkiSubnets
						.Select(IPAddressRange.Parse)
						.FirstOrDefault(x => x.Contains(IPAddress.Parse(message.Ip)))
						?.ToCidrString();
				}
				catch { }


				var groupIdWithSameDomains = db.DnsAttacks
					.Include(x => x.AttackGroup)
					.Where(x => x.BlackDomain == message.BlackDomain && x.WhiteDomain == message.WhiteDomain)
					.Select(x => x.AttackGroupId)
					.AsEnumerable()
					.DefaultIfEmpty(-1)
					.Max();

				if (groupIdWithSameDomains != -1)
				{
					var group = db.AttackGroups
						.Include(x => x.Attacks)
						.First(x => x.Id == groupIdWithSameDomains);
					if (group.Status != (int)AttackGroupStatusEnum.Complete)
					{
						var attackWithSameIp = group.Attacks
							.FirstOrDefault(x => x.Ip == message.Ip);
						if (attackWithSameIp == default)
						{
							var newAttack = new Attacks
							{
								BlackDomain = message.BlackDomain,
								WhiteDomain = message.WhiteDomain,
								Ip = message.Ip,
								Status = (int)AttackStatusEnum.Intersection
							};
							db.DnsAttacks.Add(newAttack);
							newAttack.Histories.Add(new AttackHistories
							{
								CurrentStatus = (int)AttackStatusEnum.Intersection,
								PrevStatus = (int)AttackStatusEnum.None,
								Date = DateTimeOffset.UtcNow
							});
							group.Attacks.Add(newAttack);
							newAttack.IpBlocked = ipBlocked;
							newAttack.SubnetBlocked = subnetBlocked;
							db.SaveChanges();
							notifyAttacks.Add(newAttack.Id);
						}
						else
						{
							if (attackWithSameIp.Status != (int)AttackStatusEnum.Intersection)
							{
								attackWithSameIp.Histories.Add(new AttackHistories
								{
									CurrentStatus = (int)AttackStatusEnum.Intersection,
									PrevStatus = (int)attackWithSameIp.Status,
									Date = DateTimeOffset.UtcNow
								});
								attackWithSameIp.Status = (int)AttackStatusEnum.Intersection;
								attackWithSameIp.IpBlocked = ipBlocked;
								attackWithSameIp.SubnetBlocked = subnetBlocked;
								db.SaveChanges();
								notifyAttacks.Add(attackWithSameIp.Id);
							}
						}
					}
					else
					{
						var newGroup = CreateNewGroup(db, message);
						var newAttack = newGroup.Attacks.First();
						newAttack.IpBlocked = ipBlocked;
						newAttack.SubnetBlocked = subnetBlocked;
						db.SaveChanges();
						notifyAttacks.Add(newAttack.Id);
					}
				}
				else
				{
					var newGroup = CreateNewGroup(db, message);
					var newAttack = newGroup.Attacks.First();
					newAttack.IpBlocked = ipBlocked;
					newAttack.SubnetBlocked = subnetBlocked;
					db.SaveChanges();
					notifyAttacks.Add(newAttack.Id);
				}
			}
			_logger.LogInformation("Update DNS Attacks complete");
			return notifyAttacks;
		}

		public IEnumerable<int> UpdateAttackGroup()
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			var completedAttack = new List<AttackGroups>();
			var attackGroups = db.AttackGroups
				.Where(x => x.Status != (int)AttackGroupStatusEnum.Complete)
				.Include(x => x.Attacks)
				.ToList();
			foreach (var group in attackGroups)
			{
				if (group.Attacks.Count == 0)
				{
					db.AttackGroups.Remove(group);
				}
				else
				{
					var isAllCompleted = group.Attacks.All(x => x.StatusEnum == AttackStatusEnum.Completed);
					if (isAllCompleted)
					{
						var prevStatus = group.StatusEnum;
						group.Status = (int)AttackGroupStatusEnum.Complete;
						group.DateClose = DateTimeOffset.UtcNow;
						var newHistory = AddNewAttackGroupHistory(group.StatusEnum, prevStatus);
						db.GroupHistories.Add(newHistory);
						newHistory.AttackGroupId = group.Id;
						group.GroupHistories.Add(newHistory);
						completedAttack.Add(group);
					}
				}
			}
			db.SaveChanges();
			_logger.LogInformation("Update DNS AttacksGroups complete");
			return completedAttack.Select(x => x.Id).AsEnumerable();
		}

		public IEnumerable<int> CheckForExpiredAttacks()
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			var changedAttackIds = new List<int>();

			var falsePositiveAttacks = new List<Attacks>();

			var threshold = DateTimeOffset.UtcNow.Add(-_expireInterval);
			var closingThreshold = DateTimeOffset.UtcNow.Add(-_closingInterval);
			var falsePositiveThreshold = DateTimeOffset.UtcNow.Add(-_falsePositiveInterval);
			var groups = db.AttackGroups
				.Include(x => x.Attacks)
				.Where(x => x.LastUpdate < threshold && x.Status != (int)AttackGroupStatusEnum.Complete)
				.ToList();
			foreach (var group in groups)
			{
				foreach (var attack in group.Attacks)
				{
					var lastStatus = attack.StatusEnum;
					if (lastStatus == AttackStatusEnum.Intersection)
					{
						attack.Status = (int)AttackStatusEnum.Closing;
						var newHistory = AddNewAttackHistory(attack.StatusEnum, AttackStatusEnum.Intersection);
						attack.Histories.Add(newHistory);
						db.AttackHistories.Add(newHistory);
						changedAttackIds.Add(attack.AttackGroupId);
					}
					else if (lastStatus == AttackStatusEnum.Closing && group.LastUpdate < closingThreshold)
					{
						attack.Status = (int)AttackStatusEnum.Completed;
						var newHistory = AddNewAttackHistory(attack.StatusEnum, AttackStatusEnum.Closing);
						attack.Histories.Add(newHistory);
						db.AttackHistories.Add(newHistory);
						changedAttackIds.Add(attack.AttackGroupId);
					}
					else if (lastStatus == AttackStatusEnum.Closing)
					{
						var prevChange = attack.Histories.OrderByDescending(x => x.Id).FirstOrDefault();
						if (prevChange != null)
						{
							var correctStatusToFalsePositive = prevChange.PrevStatusEnum == AttackStatusEnum.None
								|| prevChange.PrevStatusEnum == AttackStatusEnum.Intersection;
							if (correctStatusToFalsePositive)
							{
								if (prevChange.PrevStatusEnum != AttackStatusEnum.None)
								{
									var prevHistory = attack.Histories.OrderByDescending(x => x.Id).Skip(1).FirstOrDefault();
									if (prevHistory != null && prevHistory.Date > falsePositiveThreshold)
									{
										falsePositiveAttacks.Add(attack);
										changedAttackIds.Add(attack.AttackGroupId);
									}
								}
								else
								{
									if (prevChange.Date > falsePositiveThreshold)
									{
										falsePositiveAttacks.Add(attack);
										changedAttackIds.Add(attack.AttackGroupId);
									}
								}
							}
						}
					}
				}
			}

			foreach (var falseAttack in falsePositiveAttacks)
			{
				db.DnsAttacks.Remove(falseAttack);
				db.AttackGroups.Remove(falseAttack.AttackGroup);
			}

			db.SaveChanges();
			_logger.LogInformation("Check for expired attacks complete");
			return changedAttackIds;
		}

		private AttackGroups CreateNewGroup(DnsDbContext db, AttackFoundMessage message)
		{
			var newGroup = new AttackGroups
			{
				DateBegin = DateTimeOffset.UtcNow,
				LastUpdate = DateTimeOffset.UtcNow,
				Status = (int)AttackGroupStatusEnum.PendingCheck,
			};
			db.AttackGroups.Add(newGroup);
			newGroup.GroupHistories.Add(new AttackGroupHistories
			{
				CurrentStatus = (int)AttackGroupStatusEnum.PendingCheck,
				PrevStatus = (int)AttackGroupStatusEnum.None,
				Date = DateTimeOffset.UtcNow
			});
			var newAttack = new Attacks
			{
				BlackDomain = message.BlackDomain,
				WhiteDomain = message.WhiteDomain,
				Ip = message.Ip,
				Status = (int)AttackStatusEnum.Intersection
			};
			db.DnsAttacks.Add(newAttack);
			newAttack.Histories.Add(new AttackHistories
			{
				CurrentStatus = (int)AttackStatusEnum.Intersection,
				PrevStatus = (int)AttackStatusEnum.None,
				Date = DateTimeOffset.UtcNow
			});
			newGroup.Attacks.Add(newAttack);
			return newGroup;
		}

		private AttackGroups CreateNewAttackGroup()
		{
			return new AttackGroups
			{
				DateBegin = DateTimeOffset.UtcNow,
				Status = (int)AttackGroupStatusEnum.PendingCheck,
				LastUpdate = DateTimeOffset.UtcNow
			};
		}

		private Attacks AddNewAttack(AttackFoundMessage model)
		{
			return new Attacks
			{
				BlackDomain = model.BlackDomain,
				Ip = model.Ip,
				Status = (int)AttackStatusEnum.Intersection,
				WhiteDomain = model.WhiteDomain,
			};
		}

		private AttackHistories AddNewAttackHistory(AttackStatusEnum currentStatus, AttackStatusEnum prevStatus)
		{
			return new AttackHistories
			{
				Date = DateTimeOffset.UtcNow,
				CurrentStatus = (int)currentStatus,
				PrevStatus = (int)prevStatus,
			};
		}

		private AttackGroupHistories AddNewAttackGroupHistory(AttackGroupStatusEnum currentStatus, AttackGroupStatusEnum prevStatus)
		{
			return new AttackGroupHistories
			{
				Date = DateTimeOffset.UtcNow,
				CurrentStatus = (int)currentStatus,
				PrevStatus = (int)prevStatus,
			};
		}
	}
}
