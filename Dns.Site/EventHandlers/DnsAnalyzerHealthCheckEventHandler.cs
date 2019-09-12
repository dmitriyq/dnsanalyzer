using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Events;
using Dns.Site.Hubs;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Dns.Site.EventHandlers
{
	public class DnsAnalyzerHealthCheckEventHandler : IIntegrationEventHandler<DnsAnalyzerHealthCheckEvent>
	{
		private readonly IHubContext<HealthCheckHub> _hubContext;

		public DnsAnalyzerHealthCheckEventHandler(IHubContext<HealthCheckHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public async Task<bool> Handle(DnsAnalyzerHealthCheckEvent @event)
		{
			await _hubContext.Clients.All.SendAsync("Update", @event).ConfigureAwait(false);
			return true;
		}
	}
}
