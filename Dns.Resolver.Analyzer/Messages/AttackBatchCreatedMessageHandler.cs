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
		private readonly IAnalyzeService _analyzeService;
		private readonly INotifyService _notifyService;
		private readonly IMessageQueue _messageQueue;

		public AttackBatchCreatedMessageHandler(IAnalyzeService analyzeService, INotifyService notifyService, IMessageQueue messageQueue)
		{
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_messageQueue = messageQueue;
		}

		public async Task Handle(AttackBatchCreatedMessage message)
		{
			var attacks = _analyzeService.Exclude(message.AttackMessages);
			var updatedAttackIds = await _analyzeService.UpdateAttackAsync(attacks).ConfigureAwait(false);
			var updatedGroupIds = _analyzeService.UpdateAttackGroup();

			var updateMessage = new UpdatedAttackMessage(updatedAttackIds.ToList(), updatedGroupIds.ToList());
			await _messageQueue.PublishAsync(updateMessage).ConfigureAwait(false);

			if (updatedAttackIds.Any())
			{
				var attackMsg = _notifyService.BuildAttackMessage(string.Empty, updatedAttackIds.ToArray());
				await _notifyService.SendAsync(attackMsg).ConfigureAwait(false);
			}
			if (updatedGroupIds.Any())
			{
				var groupMsg = _notifyService.BuildGroupMessage(string.Empty, updatedGroupIds.ToArray());
				await _notifyService.SendAsync(groupMsg).ConfigureAwait(false);
			}
		}
	}
}
