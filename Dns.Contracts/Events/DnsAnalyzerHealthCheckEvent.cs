using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Events;

namespace Dns.Contracts.Events
{
	public class DnsAnalyzerHealthCheckEvent: IntegrationEvent
	{
		public string Service { get; }
		public string CurrentAction { get; }

		public DnsAnalyzerHealthCheckEvent(string serviceName, string currentAction)
		{
			Service = serviceName;
			CurrentAction = currentAction;
		}
	}
}
