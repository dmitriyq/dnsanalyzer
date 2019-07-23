using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Library;
using Dns.Library.Services;
using Grfc.Library.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Dns.Analyzer.Services
{
	public class Bootstrapper
	{
		private readonly ILogger<Bootstrapper> _logger;
		private readonly RedisService _redis;
		private readonly AttackService _attackService;
		private readonly IpInfoService _ipInfoService;
		private readonly SuspectDomainSevice _suspectDomainSevice;
		private readonly int SuspectIpCount = int.Parse(EnvironmentExtensions.GetVariable(EnvVars.ANALYZER_SUSPECT_IP_COUNT));

		public Bootstrapper(
			ILogger<Bootstrapper> logger,
			RedisService redisService,
			AttackService attackService,
			IpInfoService ipInfoService,
			SuspectDomainSevice suspectDomainSevice)
		{
			_logger = logger;
			_redis = redisService;
			_attackService = attackService;
			_ipInfoService = ipInfoService;
			_suspectDomainSevice = suspectDomainSevice;
		}

		public async Task StartAnalyzer()
		{
			await _redis.Subscribe(RedisKeys.RESOLVE_COMPLETE_CHANNEL, async msg =>
			{
				_logger.LogInformation("Starting Analyzer Job");

				var resolvedBlackDomain = await _redis.GetResolvedDomains(RedisKeys.BLACK_DOMAINS_RESOLVED);
				var resolvedWhiteDomain = await _redis.GetResolvedDomains(RedisKeys.WHITE_DOMAINS_RESOLVED);

				var suspectDomains = resolvedBlackDomain.Where(x => x.IPAddresses.Count > SuspectIpCount).AsEnumerable();
				await _suspectDomainSevice.UpdateSuspectDomains(suspectDomains);

				var attacks = await _attackService.FindAttacks(resolvedBlackDomain, resolvedWhiteDomain);
				attacks = await _attackService.ExcludeDomains(attacks);

				await _attackService.UpdateDnsAttacks(attacks);
				await _attackService.UpdateDnsAttackGroups();

				await _ipInfoService.UpdateIpInfo(true);

				//TODO: Send Notifications
				//await _notifyService.AttackNotitications(newerAttackIds);
				//await _notifyService.GroupNotitications(completedGroups);

				_logger.LogInformation("Completed Analyzer Job");
			});
		}
	}
}
