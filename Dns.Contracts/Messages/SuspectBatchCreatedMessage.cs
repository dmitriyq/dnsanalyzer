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

#pragma warning disable CS8618
		public SuspectBatchCreatedMessage()
#pragma warning restore CS8618
		{
		}

		public SuspectBatchCreatedMessage(ICollection<SuspectDomainFoundMessage> suspectDomains)
		{
			SuspectDomainMessages = suspectDomains.ToArray();
		}
	}
}
