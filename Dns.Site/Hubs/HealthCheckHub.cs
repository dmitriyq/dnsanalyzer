using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Dns.Site.Hubs
{
	public class HealthCheckHub: Hub
	{
		private readonly ILogger<HealthCheckHub> _logger;

		public HealthCheckHub(ILogger<HealthCheckHub> logger)
		{
			_logger = logger;
		}
	}
}
