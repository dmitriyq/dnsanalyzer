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
	public class DomainResolvedMessageHandler : IMessageQueueHandler<DomainResolvedMessage>
	{
		private readonly IDomainAggregatorService _aggregatorService;

		public DomainResolvedMessageHandler(IDomainAggregatorService aggregatorService)
		{
			_aggregatorService = aggregatorService;
		}

		public Task<bool> Handle(DomainResolvedMessage message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			_logger.LogInformation($"Handled ID {message.TraceId} - {message.Name}");
			_aggregatorService.AddDomain(message);
			return Task.FromResult(true);
		}
	}
}
