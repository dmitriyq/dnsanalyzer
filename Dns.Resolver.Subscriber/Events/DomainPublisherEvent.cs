using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Events;

namespace Dns.Resolver.Subscriber.Events
{
	public class DomainPublisherEvent: IntegrationEvent
	{
		public string Domain { get; set; }
		public int DomainType { get; set; }

		public DomainPublisherEvent(string domain, int domainType)
		{
			Domain = domain;
			DomainType = domainType;
		}
	}
}
