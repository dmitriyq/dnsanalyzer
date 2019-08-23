using System.Linq;
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
		private readonly NotifyService _notifyService;
		private readonly int SuspectIpCount = int.Parse(EnvironmentExtensions.GetVariable(EnvVars.ANALYZER_SUSPECT_IP_COUNT));

		public Bootstrapper(
			ILogger<Bootstrapper> logger,
			RedisService redisService,
			AttackService attackService,
			IpInfoService ipInfoService,
			SuspectDomainSevice suspectDomainSevice,
			NotifyService notifyService)
		{
			_logger = logger;
			_redis = redisService;
			_attackService = attackService;
			_ipInfoService = ipInfoService;
			_suspectDomainSevice = suspectDomainSevice;
			_notifyService = notifyService;
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

				var attackToNotify = await _attackService.UpdateDnsAttacks(attacks);
				var groupToNotify = await _attackService.UpdateDnsAttackGroups();

				await _ipInfoService.UpdateIpInfo(true);

				if (attackToNotify.Any())
				{
					var attackMessage = await _notifyService.BuildAttackMessage(string.Empty, attackToNotify.ToArray());
					await _redis.Publish(RedisKeys.NOTIFY_SEND_CHANNEL, attackMessage.ProtoSerialize());
				}
				if (groupToNotify.Any())
				{
					var groupMessage = await _notifyService.BuildGroupMessage(string.Empty, groupToNotify.ToArray());
					await _redis.Publish(RedisKeys.NOTIFY_SEND_CHANNEL, groupMessage.ProtoSerialize());
				}

				_logger.LogInformation("Completed Analyzer Job");
			});
		}
	}
}