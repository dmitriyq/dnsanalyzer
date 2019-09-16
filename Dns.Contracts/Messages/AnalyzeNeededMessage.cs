using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class AnalyzeNeededMessage: AmqpMessage
	{
		public Guid TraceId { get; set; }

		public AnalyzeNeededMessage(Guid traceId)
		{
			TraceId = traceId;
		}
	}
}
