using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Dns.Library.Models;
using Dns.Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTools;

namespace Dns.Analyzer.Services
{
	public class AttackService
	{
		private readonly ILogger<AttackService> _logger;
		private readonly DnsDbContext _dbContext;
		private readonly DnsReadOnlyDbContext _readOnlyDbContext;
		private readonly RedisService _redis;

		public AttackService(
			ILogger<AttackService> logger,
			DnsDbContext dbContext,
			DnsReadOnlyDbContext readOnlyDbContext,
			RedisService redis)
		{
			_logger = logger;
			_dbContext = dbContext;
			_readOnlyDbContext = readOnlyDbContext;
			_redis = redis;
		}

		public async Task<IEnumerable<DnsAttackModel>> FindAttacks(IEnumerable<ResolvedDomain> blackDomains, IEnumerable<ResolvedDomain> whiteDomains)
		{
			var attacks = new List<DnsAttackModel>();
			foreach (var whiteDomain in whiteDomains)
			{
				foreach (var whiteIp in whiteDomain.IPAddresses)
				{
					foreach (var intersection in blackDomains.Where(x => x.IPAddresses.Contains(whiteIp)))
					{
						var ip = intersection.IPAddresses.First(x => x == whiteIp);
						attacks.Add(new DnsAttackModel
						{
							BlackDomain = intersection.Name,
							WhiteDomain = whiteDomain.Name,
							Ip = ip
						});
					}
				}
			}
			return await Task.FromResult(attacks);
		}

		public async Task<IEnumerable<DnsAttackModel>> ExcludeDomains(IEnumerable<DnsAttackModel> attacks)
		{
			var excludedDomains = await _readOnlyDbContext.DomainExcludedNames.ToListAsync();
			return attacks
				.Where(x => !excludedDomains.Any(z => z.BlackDomain == x.BlackDomain && z.WhiteDomain == x.WhiteDomain))
				.AsEnumerable();
		}

		public async Task<IEnumerable<int>> UpdateDnsAttacks(IEnumerable<DnsAttackModel> attacks)
		{
			var newAttacks = new List<Attacks>();
			var ignoreAttacks = new List<Attacks>();

			var storedAttacks = await _dbContext.DnsAttacks.Include(x => x.AttackGroup).ToListAsync();
			var blockedIps = await _redis.GetStringSetMembers(RedisKeys.BLACK_IPS);
			var blockedSubnets = await _redis.GetStringSetMembers(RedisKeys.BLACK_SUBNETS);

			foreach (var recentAttack in attacks)
			{
				bool needNotify = false;
				Attacks attack = null;
				var groups = storedAttacks
					.Where(x => x.WhiteDomain == recentAttack.WhiteDomain && x.BlackDomain == recentAttack.BlackDomain)
					.Select(x => x.AttackGroup);
				if (groups.Any())
				{
					var lastGroup = groups.OrderBy(x => x.Id).LastOrDefault();
					if (lastGroup.StatusEnum == AttackGroupStatusEnum.Complete)
					{
						var group = CreateNewAttackGroup(recentAttack, blockedIps, blockedSubnets);
						_dbContext.AttackGroups.Add(group);
						lastGroup = group;
						attack = lastGroup.Attacks.LastOrDefault();
						needNotify = true;
					}
					else
					{
						attack = lastGroup.Attacks.FirstOrDefault(x => x.Ip == recentAttack.Ip);
						if (attack != null)
						{
							if (attack.StatusEnum == AttackStatusEnum.Closing || attack.StatusEnum == AttackStatusEnum.Completed)
							{
								var prevStatus = (AttackStatusEnum)attack.Status;
								attack.Status = (int)AttackStatusEnum.Intersection;
								var history = AddNewAttackHistory(attack, prevStatus);
								needNotify = true;
							}
							else ignoreAttacks.Add(attack);
						}
						else
						{
							attack = AddNewAttack(recentAttack, lastGroup);
							var history = AddNewAttackHistory(attack, AttackStatusEnum.None);
							needNotify = true;
						}
					}
				}
				else
				{
					var group = CreateNewAttackGroup(recentAttack, blockedIps, blockedSubnets);
					_dbContext.AttackGroups.Add(group);
					attack = group.Attacks.LastOrDefault();
					needNotify = true;
				}

				attack.IpBlocked = blockedIps.Any(x => x == recentAttack.Ip);
				try
				{
					attack.SubnetBlocked = blockedSubnets
						.Select(x => IPAddressRange.Parse(x))
						.FirstOrDefault(x => x.Contains(IPAddress.Parse(recentAttack.Ip)))
						?.ToCidrString();
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, ex.Message);
				}
				await _dbContext.SaveChangesAsync();
				if (needNotify)
					newAttacks.Add(attack);
			}

			var treshholdCompleted = DateTimeOffset.UtcNow.AddDays(-1);
			var newIds = newAttacks.Select(x => x.Id).ToList();
			var ignoreIds = ignoreAttacks.Select(x => x.Id).ToList();
			var allIds = storedAttacks.Select(x => x.Id).ToList();

			var notUpdatedIds = allIds.Except(newIds).Except(ignoreIds).ToList();
			var notUpdatedAttacks = storedAttacks.Where(x => notUpdatedIds.Contains(x.Id)).ToList();
			foreach (var attack in notUpdatedAttacks)
			{
				if (attack.AttackGroup.StatusEnum != AttackGroupStatusEnum.Complete)
				{
					var status = attack.StatusEnum;
					var history = _dbContext.AttackHistories.Where(x => x.AttackId == attack.Id).OrderBy(x => x.Id).LastOrDefault();
					if (status == AttackStatusEnum.Intersection)
					{
						attack.Status = (int)AttackStatusEnum.Closing;
						AddNewAttackHistory(attack, AttackStatusEnum.Intersection);
						newAttacks.Add(attack);
					}
					else if (status == AttackStatusEnum.Closing)
					{
						if (history.Date < treshholdCompleted)
						{
							attack.Status = (int)AttackStatusEnum.Completed;
							AddNewAttackHistory(attack, AttackStatusEnum.Closing);
							newAttacks.Add(attack);
						}
					}
				}
			}

			await _dbContext.SaveChangesAsync();
			_logger.LogInformation("Update DNS Attacks complete");
			return newAttacks.Select(x => x.Id).AsEnumerable();
		}

		public async Task<IEnumerable<int>> UpdateDnsAttackGroups()
		{
			var completedAttack = new List<AttackGroups>();
			var attackGroups = await _dbContext.AttackGroups
				.Where(x => x.Status != (int)AttackGroupStatusEnum.Complete)
				.Include(x => x.Attacks)
				.ToListAsync();
			foreach (var group in attackGroups)
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
			await _dbContext.SaveChangesAsync();
			_logger.LogInformation("Update DNS AttacksGroups complete");
			return completedAttack.Select(x => x.Id).AsEnumerable();
		}

		private AttackGroups CreateNewAttackGroup(DnsAttackModel model, IEnumerable<string> blockedIps, IEnumerable<string> blockedSubnets)
		{
			var newGroup = new AttackGroups { DateBegin = DateTimeOffset.UtcNow, Status = (int)AttackGroupStatusEnum.PendingCheck };
			var groupHistory = AddNewAttackGroupHistory(newGroup, AttackGroupStatusEnum.None);
			var attack = AddNewAttack(model, newGroup);
			var history = AddNewAttackHistory(attack, AttackStatusEnum.None);
			return newGroup;
		}

		private Attacks AddNewAttack(DnsAttackModel model, AttackGroups attackGroups)
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
	}
}