using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dns.Analyzer.Models;
using Dns.Contracts.Protobuf;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTools;

namespace Dns.Analyzer.Services
{
	public class AnalyzeService : IAnalyzeService
	{
		private readonly ILogger<AnalyzeService> _logger;
		private readonly DnsDbContext _dbContext;
		public AnalyzeService(ILogger<AnalyzeService> logger, DnsDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<AttackModel>> ExcludeDomainsAsync(IEnumerable<AttackModel> attacks)
		{
			var excludedDomains = await _dbContext.DomainExcludedNames.ToListAsync().ConfigureAwait(false);
			return attacks
				.Where(x => !excludedDomains.Any(z => z.BlackDomain == x.BlackDomain && z.WhiteDomain == x.WhiteDomain))
				.AsEnumerable();
		}

		public IEnumerable<AttackModel> FindIntersection(IEnumerable<ResolvedDomain> blackDomains, IEnumerable<ResolvedDomain> whiteDomains)
		{
			var intersectionList = new List<AttackModel>();
			foreach (var whiteDomain in whiteDomains)
			{
				foreach (var whiteIp in whiteDomain.IPAddresses)
				{
					foreach (var blackDomain in blackDomains.Where(x => x.IPAddresses.Contains(whiteIp)))
					{
						var ip = blackDomain.IPAddresses.First(x => x == whiteIp);
						intersectionList.Add(new AttackModel(blackDomain.Name, whiteDomain.Name, ip));
					}
				}
			}
			return intersectionList;
		}

		public async Task<IEnumerable<int>> UpdateAttacksAsync(IEnumerable<AttackModel> attacks, ISet<string> vigruzkiIps, ISet<string> vigruzkiSubnets)
		{
			var newAttacks = new List<Attacks>();
			var ignoreAttacks = new List<Attacks>();

			var storedAttacks = await _dbContext.DnsAttacks.Include(x => x.AttackGroup).ToListAsync();

			foreach (var attack in attacks)
			{
				bool needNotify = false;
				Attacks? storedAttack = null;

				var similarGroups = storedAttacks
					.Where(x => x.BlackDomain == attack.BlackDomain && x.WhiteDomain == attack.WhiteDomain)
					.Select(x => x.AttackGroup);
				bool existingGroup = similarGroups.Any();
				if (existingGroup)
				{
					var latestGroup = similarGroups.OrderBy(x => x.Id).Last();
					bool isCompletedAttack = latestGroup.StatusEnum == AttackGroupStatusEnum.Complete;
					if (isCompletedAttack)
					{
						var group = CreateNewAttackGroup(attack);
						_dbContext.AttackGroups.Add(group);
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
							else
							{
								ignoreAttacks.Add(storedAttack);
							}
						}
						else
						{
							storedAttack = AddNewAttack(attack, latestGroup);
							var history = AddNewAttackHistory(storedAttack, AttackStatusEnum.None);
							needNotify = true;
						}
					}
				}
				else
				{
					var group = CreateNewAttackGroup(attack);
					_dbContext.AttackGroups.Add(group);
					storedAttack = group.Attacks.Last();
					needNotify = true;
				}

				if (storedAttack != null)
				{
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
					_dbContext.Entry(storedAttack).State = EntityState.Modified;
					if (needNotify)
						newAttacks.Add(storedAttack);
				}
			}
			//try-catch
			await _dbContext.SaveChangesAsync();
			//check not updated attacks  
		}

		private AttackGroups CreateNewAttackGroup(AttackModel model)
		{
			var newGroup = new AttackGroups { DateBegin = DateTimeOffset.UtcNow, Status = (int)AttackGroupStatusEnum.PendingCheck };
			_ = AddNewAttackGroupHistory(newGroup, AttackGroupStatusEnum.None);
			var attack = AddNewAttack(model, newGroup);
			_ = AddNewAttackHistory(attack, AttackStatusEnum.None);
			return newGroup;
		}

		private Attacks AddNewAttack(AttackModel model, AttackGroups attackGroups)
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
