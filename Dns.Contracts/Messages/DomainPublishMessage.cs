using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class DomainPublishMessage : AmqpMessage
	{
		public string Domain { get; set; }
		public int DomainType { get; set; }
		public Guid TraceId { get; set; }

		public DomainPublishMessage(string domain, int domainType, Guid traceId)
		{
			Domain = domain;
			DomainType = domainType;
			TraceId = traceId;
		}
	}
}
