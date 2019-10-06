using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class SuspectDomainFoundMessage: AmqpMessage
	{
		public string Domain { get; set; }
		public HashSet<string> IpAddresses { get; set; }

		public SuspectDomainFoundMessage(string domain, HashSet<string> ips)
		{
			Domain = domain;
			IpAddresses = ips;
		}
	}
}
