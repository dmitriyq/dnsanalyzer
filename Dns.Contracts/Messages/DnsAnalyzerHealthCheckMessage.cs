using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class DnsAnalyzerHealthCheckMessage: AmqpMessage
	{
		public string Service { get; set; }
		public string CurrentAction { get; set; }

		public DnsAnalyzerHealthCheckMessage(string serviceName, string currentAction)
		{
			Service = serviceName;
			CurrentAction = currentAction;
		}
	}
}
