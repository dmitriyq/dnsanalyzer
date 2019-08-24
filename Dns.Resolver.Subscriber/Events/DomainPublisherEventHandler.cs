using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Resolver.Subscriber.Services;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Subscriber.Events
{
	public class DomainPublisherEventHandler : IIntegrationEventHandler<DomainPublisherEvent>
	{
		private readonly ILogger<DomainPublisherEventHandler > _logger;
		private readonly IEventBus _eventBus;
		private readonly IDomainLookupService _domainLookup;

		public DomainPublisherEventHandler (ILogger<DomainPublisherEventHandler > logger,
			IEventBus eventBus, IDomainLookupService domainLookup)
		{
			_logger = logger;
			_eventBus = eventBus;
			_domainLookup = domainLookup;
		}

		public async Task Handle(DomainPublisherEvent @event)
		{
			_logger.LogInformation($"Handle event {@event.Id} - {@event}");

			var ips = await _domainLookup.GetIpAddressesAsync(@event.Domain).ConfigureAwait(false);

			_logger.LogInformation($"Publish resolve result: {@event.Id} - {@event}");
			_eventBus.Publish(new DomainResolveCompleteEvent(@event.Domain, ips, @event.DomainType));
		}
	}
}
