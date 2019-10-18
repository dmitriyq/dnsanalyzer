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
		private readonly IMessageQueue _messageQueue;

		public AttackFoundMessageHandler(ILogger<AttackFoundMessageHandler> logger, IAnalyzeService analyzeService, INotifyService notifyService,
			ConnectionMultiplexer redis, IMessageQueue messageQueue, string notifyChannel)
		{
			_logger = logger;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_redis = redis.GetDatabase();
			_messageQueue = messageQueue;
			_notifyChannel = notifyChannel;
		}

		public async Task Handle(AttackFoundMessage message)
		{
			_logger.LogInformation($"Handle message {message.WhiteDomain} - {message.BlackDomain}");
			await SendHealthCheckAsync($"Обновление атаки [{message.WhiteDomain} - {message.BlackDomain} - {message.Ip}]").ConfigureAwait(true);
			if (!await _analyzeService.IsExcludedAsync(message).ConfigureAwait(true))
			{
				var attackId = await _analyzeService.UpdateAttackAsync(message).ConfigureAwait(true);
				if (attackId != null)
				{
					var redisMsg = _notifyService.BuildAttackMessage(string.Empty, attackId.Value);
					await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(true);
				}
				var groupIds = await _analyzeService.UpdateAttackGroupAsync(message).ConfigureAwait(true);
				if (groupIds.Any())
				{
					var redisMsg = _notifyService.BuildAttackMessage(string.Empty, groupIds.ToArray());
					await _redis.PublishAsync(_notifyChannel, redisMsg.ProtoSerialize()).ConfigureAwait(true);
				}
			}
		}

		private Task SendHealthCheckAsync(string action)
			=> _messageQueue.PublishAsync(new DnsAnalyzerHealthCheckMessage(typeof(Program).Namespace!, action));
	}
}
