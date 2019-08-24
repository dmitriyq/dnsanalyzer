using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Events;

namespace Dns.Resolver.Subscriber.Events
{
	public class DomainResolveCompleteEvent: IntegrationEvent
	{
		public string Domain { get; set; }
		public ISet<string> IpAddresses { get; set; }
		public int DomainType { get; set; }

		public DomainResolveCompleteEvent(string domain, ISet<string> ips, int domainType)
		{
			Domain = domain;
			IpAddresses = ips;
			DomainType = domainType;
		}
	}
}
