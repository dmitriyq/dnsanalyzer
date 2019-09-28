using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class DomainUnresolvedMessage: AmqpMessage, ITraceable
	{
		public string Domain { get; set; }
		public DomainResolveErrorType ResolveErrorType { get; set; }

		public Guid TraceId { get; set; }

		public DomainUnresolvedMessage(string domain, DomainResolveErrorType errorType, Guid traceId)
		{
			Domain = domain;
			ResolveErrorType = errorType;
			TraceId = traceId;
		}
	}

	public enum DomainResolveErrorType
	{
		NotExist = 1,
		ServerFail = 2,
		RequestTimeout = 3,
		WithoutARecords = 4,
	}
}
