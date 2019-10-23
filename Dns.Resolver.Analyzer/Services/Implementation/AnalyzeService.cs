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

		public async Task<AttackFoundMessage[]> ExcludeAsync(AttackFoundMessage[] attacks)
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			var excludedDomains = await db.DomainExcludedNames.ToListAsync().ConfigureAwait(true);
			return attacks
				.Where(x => !excludedDomains.Any(z => z.BlackDomain == x.BlackDomain && z.WhiteDomain == x.WhiteDomain))
				.ToArray();
		}

		public async Task<IEnumerable<int>> UpdateAttackAsync(AttackFoundMessage[] attacks)
		{
			var newAttacks = new List<Attacks>();

			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			var storedAttacks = await db.DnsAttacks.Include(x => x.AttackGroup).ToListAsync().ConfigureAwait(true);

			var vigruzkiIps = await _cacheService.GetVigruzkiIps().ConfigureAwait(true);
			var vigruzkiSubnets = await _cacheService.GetVigruzkiSubnets().ConfigureAwait(true);

			foreach (var recentAttack in attacks)
			{
				bool needNotify = false;
				Attacks? storedAttack = null;

				var similarGroups = storedAttacks
				.Where(x => x.BlackDomain == recentAttack.BlackDomain && x.WhiteDomain == recentAttack.WhiteDomain)
				.Select(x => x.AttackGroup).ToList();

				bool existingGroup = similarGroups.Count > 0;
				if (existingGroup)
				{
					var latestGroup = similarGroups.OrderByDescending(x => x.Id).First();
					bool isCompletedAttack = latestGroup.StatusEnum == AttackGroupStatusEnum.Complete;
					if (isCompletedAttack)
					{
						var newGroup = CreateNewAttackGroup();
						db.Entry(newGroup).State = EntityState.Added;
						var newGroupHistory = AddNewAttackGroupHistory(newGroup.StatusEnum, AttackGroupStatusEnum.None);
						newGroup.GroupHistories.Add(newGroupHistory);
						var newAttack = AddNewAttack(recentAttack);
						var newAttackHistory = AddNewAttackHistory(newAttack.StatusEnum, AttackStatusEnum.None);
						newAttack.Histories.Add(newAttackHistory);
						newGroup.Attacks.Add(newAttack);
						db.AttackGroups.Add(newGroup);
						latestGroup = newGroup;
						storedAttack = latestGroup.Attacks.Last();
						needNotify = true;
					}
					else
					{
						storedAttack = latestGroup.Attacks.FirstOrDefault(x => x.Ip == recentAttack.Ip);
						bool existingAttack = storedAttack != null;
						if (existingAttack)
						{
							bool hasChanges = storedAttack!.StatusEnum != AttackStatusEnum.Intersection;
							if (hasChanges)
							{
								var prevStatus = (AttackStatusEnum)storedAttack.Status;
								storedAttack.Status = (int)AttackStatusEnum.Intersection;
								var newHistory = AddNewAttackHistory(storedAttack.StatusEnum, prevStatus);
								storedAttack.Histories.Add(newHistory);
								needNotify = true;
							}
						}
						else
						{
							storedAttack = AddNewAttack(recentAttack);
							var newHistory = AddNewAttackHistory(storedAttack.StatusEnum, AttackStatusEnum.None);
							storedAttack.Histories.Add(newHistory);
							db.Entry(storedAttack).State = EntityState.Added;
							needNotify = true;
						}
						latestGroup.LastUpdate = DateTimeOffset.UtcNow;
					}
				}
				else
				{
					var newGroup = CreateNewAttackGroup();
					db.Entry(newGroup).State = EntityState.Added;
					var newGroupHistory = AddNewAttackGroupHistory(newGroup.StatusEnum, AttackGroupStatusEnum.None);
					newGroup.GroupHistories.Add(newGroupHistory);
					var newAttack = AddNewAttack(recentAttack);
					var newAttackHistory = AddNewAttackHistory(newAttack.StatusEnum, AttackStatusEnum.None);
					newAttack.Histories.Add(newAttackHistory);
					newGroup.Attacks.Add(newAttack);
					db.AttackGroups.Add(newGroup);
					storedAttack = newGroup.Attacks.Last();
					needNotify = true;
				}
				if (storedAttack != null)
				{
					storedAttack.IpBlocked = vigruzkiIps.Contains(recentAttack.Ip);
					try
					{
						storedAttack.SubnetBlocked = vigruzkiSubnets
							.Select(IPAddressRange.Parse)
							.FirstOrDefault(x => x.Contains(IPAddress.Parse(recentAttack.Ip)))
							?.ToCidrString();
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, ex.Message);
					}
					try
					{
						await db.SaveChangesAsync().ConfigureAwait(true);
					}
					catch (DbUpdateConcurrencyException e)
					{
						_logger.LogWarning(e, e.Message);
						await db.SaveChangesAsync().ConfigureAwait(true);
					}
					catch (Exception e)
					{
						_logger.LogCritical(e, e.Message);
						throw;
					}
					if (needNotify)
						newAttacks.Add(storedAttack);
				}
			}
			_logger.LogInformation("Update DNS Attacks complete");
			return newAttacks.Select(x => x.Id).AsEnumerable();
		}

		public async Task<IEnumerable<int>> UpdateAttackGroupAsync()
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			var completedAttack = new List<AttackGroups>();
			var attackGroups = await db.AttackGroups
				.Where(x => x.Status != (int)AttackGroupStatusEnum.Complete)
				.Include(x => x.Attacks)
				.ToListAsync().ConfigureAwait(true);
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
						newHistory.AttackGroupId = group.Id;
						group.GroupHistories.Add(newHistory);
						db.Entry(newHistory).State = EntityState.Added;
						completedAttack.Add(group);
					}
				}
			}
			await db.SaveChangesAsync().ConfigureAwait(true);
			_logger.LogInformation("Update DNS AttacksGroups complete");
			return completedAttack.Select(x => x.Id).AsEnumerable();
		}

		public async Task<IEnumerable<int>> CheckForExpiredAttacksAsync()
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			var changedAttackIds = new List<int>();

			var falsePositiveAttacks = new List<Attacks>();

			var threshold = DateTimeOffset.UtcNow.Add(-_expireInterval);
			var closingThreshold = DateTimeOffset.UtcNow.Add(-_closingInterval);
			var falsePositiveThreshold = DateTimeOffset.UtcNow.Add(-_falsePositiveInterval);
			var groups = await db.AttackGroups
				.Include(x => x.Attacks)
				.Where(x => x.LastUpdate < threshold && x.Status != (int)AttackGroupStatusEnum.Complete)
				.ToListAsync()
				.ConfigureAwait(true);
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
						changedAttackIds.Add(attack.AttackGroupId);
					}
					else if (lastStatus == AttackStatusEnum.Closing && group.LastUpdate < closingThreshold)
					{
						attack.Status = (int)AttackStatusEnum.Completed;
						var newHistory = AddNewAttackHistory(attack.StatusEnum, AttackStatusEnum.Closing);
						attack.Histories.Add(newHistory);
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

			await db.SaveChangesAsync().ConfigureAwait(true);
			_logger.LogInformation("Check for expired attacks complete");
			return changedAttackIds;
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
