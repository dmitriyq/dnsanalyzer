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
		private readonly ILogger<DomainResolvedMessageHandler> _logger;

		public DomainResolvedMessageHandler(IDomainAggregatorService aggregatorService, ILogger<DomainResolvedMessageHandler> logger)
		{
			_aggregatorService = aggregatorService;
			_logger = logger;
		}

		public Task<bool> Handle(DomainResolvedMessage message)
		{
			_logger.LogInformation($"Handled message: {message.TraceId}");
			_aggregatorService.AddDomain(message);
			return Task.FromResult(true);
		}
	}
}
