using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Site.Hubs;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Dns.Site.EventHandlers
{
	public class DnsAnalyzerHealthCheckMessageHandler: IAmqpMessageHandler<DnsAnalyzerHealthCheckMessage>
	{
		private readonly IHubContext<HealthCheckHub> _hubContext;

		public DnsAnalyzerHealthCheckMessageHandler(IHubContext<HealthCheckHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public Task Handle(DnsAnalyzerHealthCheckMessage message) => _hubContext.Clients.All.SendAsync("Update", message);
	}
}
