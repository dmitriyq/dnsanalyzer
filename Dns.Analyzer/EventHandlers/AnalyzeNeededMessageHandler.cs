using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Analyzer.Models;
using Dns.Analyzer.Services;
using Dns.Contracts.Messages;
using Dns.Contracts.Protobuf;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Analyzer.EventHandlers
{
	public class AnalyzeNeededMessageHandler : IMessageQueueHandler<AnalyzeNeededMessage>
	{
		private readonly ILogger<AnalyzeNeededMessageHandler> _logger;
		private readonly IAnalyzeService _analyzeService;
		private readonly IIpInfoService _ipInfoService;
		private readonly INotifyService _notifyService;
		private readonly SuspectDomainSevice _suspectDomainSevice;
		private readonly IDatabase _redisDb;
		private readonly RedisKeys _redisKeys;
		private readonly IMessageQueue _messageQueue;
		private readonly string _healthQueue;

		public AnalyzeNeededMessageHandler(ILogger<AnalyzeNeededMessageHandler> logger, IAnalyzeService analyzeService, INotifyService notifyService,
			IIpInfoService ipInfoService, SuspectDomainSevice suspectDomainSevice, ConnectionMultiplexer redis, RedisKeys redisKeys,
			IMessageQueue messageQueue, string healthQueue)
		{
			_logger = logger;
			_analyzeService = analyzeService;
			_redisDb = redis.GetDatabase();
			_redisKeys = redisKeys;
			_suspectDomainSevice = suspectDomainSevice;
			_ipInfoService = ipInfoService;
			_notifyService = notifyService;
			_messageQueue = messageQueue;
			_healthQueue = healthQueue;
		}

		public async Task<bool> Handle(AnalyzeNeededMessage message)
		{
			_logger.LogInformation("Starting analyze");
			_messageQueue.Enqueue(new DnsAnalyzerHealthCheckMessage("Dns.Analyzer", "Начат анализ DNS-атак"), _healthQueue);
			try
			{
				var redisVigruzkiIps = await _redisDb.SetMembersAsync(_redisKeys.VigruzkiIpKey).ConfigureAwait(false);
				var vigruzkiIps = redisVigruzkiIps.ToStringArray().ToHashSet();

				var redisVigruzkiSubnets = await _redisDb.SetMembersAsync(_redisKeys.VigruzkiSubnetKey).ConfigureAwait(false);
				var vigruzkiSubnets = redisVigruzkiSubnets.ToStringArray().ToHashSet();

				var redisResolvedBlackDomains = await _redisDb.ListRangeAsync(_redisKeys.BlackDomainsResolvedKey).ConfigureAwait(false);
				var resolvedBlackDomains = redisResolvedBlackDomains
					.Select(x => ((byte[])x).ProtoDeserialize<ResolvedDomain>())
					.ToArray();
				var redisResolvedWhiteDomains = await _redisDb.ListRangeAsync(_redisKeys.WhiteDomainsResolvedKey).ConfigureAwait(false);
				var resolvedWhiteDomains = redisResolvedWhiteDomains
					.Select(x => ((byte[])x).ProtoDeserialize<ResolvedDomain>())
					.ToArray();

				await _suspectDomainSevice.UpdateSuspectDomains(resolvedBlackDomains).ConfigureAwait(false);
				var attacks = _analyzeService.FindIntersection(resolvedBlackDomains, resolvedWhiteDomains);
				attacks = await _analyzeService.ExcludeDomainsAsync(attacks).ConfigureAwait(false);

				var attackToNotify = await _analyzeService.UpdateAttacksAsync(attacks, vigruzkiIps, vigruzkiSubnets).ConfigureAwait(false);
				var groupToNotify = await _analyzeService.UpdateAttackGroupsAsync().ConfigureAwait(false);

				await _ipInfoService.UpdateIpInfoAsync(true).ConfigureAwait(false);

				if (attackToNotify.Any())
				{
					var attackMessage = await _notifyService.BuildAttackMessage(string.Empty, attackToNotify.ToArray()).ConfigureAwait(false);
					await _redisDb.PublishAsync(_redisKeys.NotificationSendMessageKey, attackMessage.ProtoSerialize()).ConfigureAwait(false);
				}
				if (groupToNotify.Any())
				{
					var groupMessage = await _notifyService.BuildGroupMessage(string.Empty, groupToNotify.ToArray()).ConfigureAwait(false);
					await _redisDb.PublishAsync(_redisKeys.NotificationSendMessageKey, groupMessage.ProtoSerialize()).ConfigureAwait(false);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
				return false;
			}
			finally
			{
				_logger.LogInformation("Analyze complete");
				_messageQueue.Enqueue(new DnsAnalyzerHealthCheckMessage("Dns.Analyzer", "Анализ DNS-атак завершен"), _healthQueue);
			}
		}
	}
}
