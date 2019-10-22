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
	public class AttackBatchCreatedMessageHandler : IAmqpMessageHandler<AttackBatchCreatedMessage>
	{
		private readonly ILogger<AttackBatchCreatedMessageHandler> _logger;
		private readonly IAnalyzeService _analyzeService;
		private readonly INotifyService _notifyService;
		private readonly IDatabase _redis;
		private readonly string _notifyChannel;

		public AttackBatchCreatedMessageHandler(ILogger<AttackBatchCreatedMessageHandler> logger, IAnalyzeService analyzeService, INotifyService notifyService,
			ConnectionMultiplexer redis, string notifyChannel)
		{
			_logger = logger;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_redis = redis.GetDatabase();
			_notifyChannel = notifyChannel;
		}

		public async Task Handle(AttackBatchCreatedMessage message)
		{
			var attacks = await _analyzeService.ExcludeAsync(message.AttackMessages).ConfigureAwait(false);
			var updatedAttackIds = await _analyzeService.UpdateAttackAsync(attacks).ConfigureAwait(false);
			var updatedGroupIds = await _analyzeService.UpdateAttackGroupAsync().ConfigureAwait(false);

			if (updatedAttackIds.Any())
			{
				var redisMsg = _notifyService.BuildAttackMessage(string.Empty, updatedAttackIds.ToArray());
				await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(true);
			}
			if (updatedGroupIds.Any())
			{
				var redisMsg = _notifyService.BuildGroupMessage(string.Empty, updatedGroupIds.ToArray());
				await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(true);
			}
			_logger.LogInformation("Batch update complete");
		}
	}
}
