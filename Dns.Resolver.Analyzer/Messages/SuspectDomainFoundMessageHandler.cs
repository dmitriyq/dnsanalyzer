using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Analyzer.Messages
{
	public class SuspectDomainFoundMessageHandler : IAmqpMessageHandler<SuspectDomainFoundMessage>
	{
		private readonly ILogger<SuspectDomainFoundMessageHandler> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly IBatchingService<SuspectDomainFoundMessage> _batchingAttack;

		public SuspectDomainFoundMessageHandler(ILogger<SuspectDomainFoundMessageHandler> logger, IMessageQueue messageQueue, IBatchingService<SuspectDomainFoundMessage> batchingAttack)
		{
			_logger = logger;
			_messageQueue = messageQueue;
			_batchingAttack = batchingAttack;
		}

		public Task Handle(SuspectDomainFoundMessage message)
		{
			_batchingAttack.Add(message);
			return Task.CompletedTask;
		}
	}
}
