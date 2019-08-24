using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Events;

namespace Dns.Resolver.Publisher.Events
{
	public class DomainPublisherEvent: IntegrationEvent
	{
		public string Domain { get; set; }
		public int DomainType { get; set; }

		public DomainPublisherEvent(string domain, int type)
		{
			Domain = domain;
			DomainType = type;
		}
	}
}
