using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class AttackFoundMessage: AmqpMessage
	{
		public string BlackDomain { get; set; }
		public string WhiteDomain { get; set; }
		public string Ip { get; set; }

		public AttackFoundMessage(string blackDomain, string whiteDomain, string ip)
		{
			BlackDomain = blackDomain;
			WhiteDomain = whiteDomain;
			Ip = ip;
		}
	}
}
