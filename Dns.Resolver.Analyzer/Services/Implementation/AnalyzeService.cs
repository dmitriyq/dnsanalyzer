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

		public async Task<bool> IsExcludedAsync(AttackFoundMessage attack)
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			var isExcluded = await db.DomainExcludedNames
				.AnyAsync(x => x.BlackDomain == attack.BlackDomain && x.WhiteDomain == attack.WhiteDomain);
			return isExcluded;
		}

		public async Task<int?> UpdateAttackAsync(AttackFoundMessage attack)
		{
			_logger.LogInformation($"Update DNS Attacks for [{attack.WhiteDomain} - {attack.BlackDomain} - {attack.Ip}]");
			bool needNotify = false;
			Attacks? storedAttack = null;
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();

			var attacks = db.DnsAttacks.Include(x => x.AttackGroup);

			var similarGroup = attacks
				.Where(x => x.BlackDomain == attack.BlackDomain && x.WhiteDomain == attack.WhiteDomain)
				.Select(x => x.AttackGroup);

			bool existingGroup = similarGroup.Any();
			if (existingGroup)
			{
				var latestGroup = similarGroup.OrderBy(x => x.Id).Last();
				bool isCompletedAttack = latestGroup.StatusEnum == AttackGroupStatusEnum.Complete;
				if (isCompletedAttack)
				{
					var group = CreateNewAttackGroup(attack);
					db.AttackGroups.Add(group);
					latestGroup = group;
					storedAttack = latestGroup.Attacks.Last();
					needNotify = true;
				}
				else
				{
					storedAttack = latestGroup.Attacks.FirstOrDefault(x => x.Ip == attack.Ip);
					bool existingAttack = storedAttack != null;
					if (existingAttack)
					{
						bool hasChanges = storedAttack!.StatusEnum != AttackStatusEnum.Intersection;
						if (hasChanges)
						{
							var prevStatus = (AttackStatusEnum)storedAttack.Status;
							storedAttack.Status = (int)AttackStatusEnum.Intersection;
							var history = AddNewAttackHistory(storedAttack, prevStatus);
							needNotify = true;
						}
					}
					else
					{
						storedAttack = AddNewAttack(attack, latestGroup);
						var history = AddNewAttackHistory(storedAttack, AttackStatusEnum.None);
						needNotify = true;
					}
					latestGroup.LastUpdate = DateTimeOffset.UtcNow;
				}
			}
			else
			{
				var group = CreateNewAttackGroup(attack);
				db.AttackGroups.Add(group);
				storedAttack = group.Attacks.Last();
				needNotify = true;
			}

			if (storedAttack != null)
			{
				var vigruzkiIps = await _cacheService.GetVigruzkiIps().ConfigureAwait(true);
				var vigruzkiSubnets = await _cacheService.GetVigruzkiSubnets().ConfigureAwait(true);
				storedAttack.IpBlocked = vigruzkiIps.Contains(attack.Ip);
				try
				{
					storedAttack.SubnetBlocked = vigruzkiSubnets
						.Select(IPAddressRange.Parse)
						.FirstOrDefault(x => x.Contains(IPAddress.Parse(attack.Ip)))
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
					return storedAttack.Id;
			}
			return null;
		}

		public async Task<IEnumerable<int>> UpdateAttackGroupAsync(AttackFoundMessage attack)
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
						AddNewAttackGroupHistory(group, prevStatus);
						completedAttack.Add(group);
					}
				}
			}
			await db.SaveChangesAsync().ConfigureAwait(true);
			_logger.LogInformation("Update DNS AttacksGroups complete");
			return completedAttack.Select(x => x.Id).AsEnumerable();
		}

		private AttackGroups CreateNewAttackGroup(AttackFoundMessage model)
		{
			var newGroup = new AttackGroups
			{
				DateBegin = DateTimeOffset.UtcNow,
				Status = (int)AttackGroupStatusEnum.PendingCheck,
				LastUpdate = DateTimeOffset.UtcNow
			};
			_ = AddNewAttackGroupHistory(newGroup, AttackGroupStatusEnum.None);
			var attack = AddNewAttack(model, newGroup);
			_ = AddNewAttackHistory(attack, AttackStatusEnum.None);
			return newGroup;
		}

		private Attacks AddNewAttack(AttackFoundMessage model, AttackGroups attackGroups)
		{
			var attack = new Attacks
			{
				BlackDomain = model.BlackDomain,
				Ip = model.Ip,
				Status = (int)AttackStatusEnum.Intersection,
				WhiteDomain = model.WhiteDomain,
				AttackGroup = attackGroups,
				AttackGroupId = attackGroups.Id
			};
			attackGroups.Attacks.Add(attack);
			return attack;
		}

		private AttackHistories AddNewAttackHistory(Attacks attack, AttackStatusEnum prevStatus)
		{
			var history = new AttackHistories
			{
				Date = DateTimeOffset.UtcNow,
				CurrentStatus = attack.Status,
				PrevStatus = (int)prevStatus,
				Attack = attack,
				AttackId = attack.Id
			};
			attack.Histories.Add(history);
			return history;
		}

		private AttackGroupHistories AddNewAttackGroupHistory(AttackGroups attackGroups, AttackGroupStatusEnum prevStatus)
		{
			var history = new AttackGroupHistories
			{
				Date = DateTimeOffset.UtcNow,
				CurrentStatus = attackGroups.Status,
				PrevStatus = (int)prevStatus,
				AttackGroup = attackGroups,
				AttackGroupId = attackGroups.Id
			};
			attackGroups.GroupHistories.Add(history);
			return history;
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
						AddNewAttackHistory(attack, AttackStatusEnum.Intersection);
						changedAttackIds.Add(attack.Id);
					}
					else if (lastStatus == AttackStatusEnum.Closing && group.LastUpdate < closingThreshold)
					{
						attack.Status = (int)AttackStatusEnum.Completed;
						AddNewAttackHistory(attack, AttackStatusEnum.Closing);
						changedAttackIds.Add(attack.Id);
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
										changedAttackIds.Add(attack.Id);
									}
								}
								else
								{
									if (prevChange.Date > falsePositiveThreshold)
									{
										falsePositiveAttacks.Add(attack);
										changedAttackIds.Add(attack.Id);
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
	}
}
