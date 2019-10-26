using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class SuspectBatchCreatedMessage: AmqpMessage
	{
		public SuspectDomainFoundMessage[] SuspectDomainMessages { get; set; }

		public SuspectBatchCreatedMessage(SuspectDomainFoundMessage[] suspectDomains)
		{
			SuspectDomainMessages = suspectDomains.ToArray();
		}
	}
}
