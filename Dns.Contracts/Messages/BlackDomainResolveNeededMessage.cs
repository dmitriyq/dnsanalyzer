using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class BlackDomainResolveNeededMessage: AmqpMessage
	{
		public string Domain { get; set; }

		public BlackDomainResolveNeededMessage(string domain)
		{
			Domain = domain;
		}
	}
}
