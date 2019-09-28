using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Aggregator.Services;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Aggregator.Messages
{
	public class DomainUnresolvedMessageHandler : IAmqpMessageHandler<DomainUnresolvedMessage>
	{
		private readonly IDomainAggregatorService _aggregatorService;
		private readonly ILogger<DomainUnresolvedMessageHandler> _logger;

		public DomainUnresolvedMessageHandler(IDomainAggregatorService aggregatorService, ILogger<DomainUnresolvedMessageHandler> logger)
		{
			_aggregatorService = aggregatorService;
			_logger = logger;
		}
		public Task Handle(DomainUnresolvedMessage message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			_aggregatorService.AddDomain(message);
			return Task.CompletedTask;
		}
	}
}
