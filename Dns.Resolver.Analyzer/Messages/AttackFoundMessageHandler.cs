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
		private readonly IMessageQueue _messageQueue;
		private readonly IBatchingService<AttackFoundMessage> _batchingAttack;

		public AttackFoundMessageHandler(IMessageQueue messageQueue, IBatchingService<AttackFoundMessage> batchingAttack)
		{
			_messageQueue = messageQueue;
			_batchingAttack = batchingAttack;
		}

		public async Task Handle(AttackFoundMessage message)
		{
			_batchingAttack.Add(message);
			await SendHealthCheckAsync($"Обновление атаки [{message.WhiteDomain} - {message.BlackDomain} - {message.Ip}]").ConfigureAwait(true);
		}

		private Task SendHealthCheckAsync(string action)
			=> _messageQueue.PublishAsync(new DnsAnalyzerHealthCheckMessage(typeof(Program).Namespace!, action));
	}
}
