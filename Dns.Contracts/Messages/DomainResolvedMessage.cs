using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class DomainResolvedMessage : AmqpMessage, ITraceable
	{
		public string Name { get; set; }
		public int DomainType { get; set; }
		public ISet<string> IPAddresses { get; set; }
		public Guid TraceId { get; set; }

		public DomainResolvedMessage(string name, int domainType, ISet<string> ips, Guid traceId)
		{
			Name = name;
			DomainType = domainType;
			IPAddresses = ips;
			TraceId = traceId;
		}
	}
}
