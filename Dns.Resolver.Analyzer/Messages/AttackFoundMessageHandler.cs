using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Contracts.Services;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Analyzer.Messages
{
	public class AttackFoundMessageHandler : IAmqpMessageHandler<AttackFoundMessage>
	{
		private readonly ILogger<AttackFoundMessageHandler> _logger;
		private readonly IAnalyzeService _analyzeService;
		private readonly INotifyService _notifyService;
		private readonly IDatabase _redis;
		private readonly string _notifyChannel;

		public AttackFoundMessageHandler(ILogger<AttackFoundMessageHandler> logger, IAnalyzeService analyzeService, INotifyService notifyService,
			ConnectionMultiplexer redis, string notifyChannel)
		{
			_logger = logger;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_redis = redis.GetDatabase();
			_notifyChannel = notifyChannel;
		}

		public async Task Handle(AttackFoundMessage message)
		{
			_logger.LogInformation($"Handle message {message.WhiteDomain} - {message.BlackDomain}");
			if (!await _analyzeService.IsExcludedAsync(message).ConfigureAwait(false))
			{
				var attackId = await _analyzeService.UpdateAttackAsync(message).ConfigureAwait(false);
				if (attackId != null)
				{
					var redisMsg = await _notifyService.BuildAttackMessage(string.Empty, attackId.Value).ConfigureAwait(false);
					await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(false);
				}
				var groupIds = await _analyzeService.UpdateAttackGroupAsync(message).ConfigureAwait(false);
				if (groupIds.Any())
				{
					var redisMsg = await _notifyService.BuildAttackMessage(string.Empty, groupIds.ToArray()).ConfigureAwait(false);
					await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(false);
				}
			}
		}
	}
}
