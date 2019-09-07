using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Events;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Analyzer.EventHandlers
{
	public class AnalyzeStartingEventHandler : IIntegrationEventHandler<AnalyzeStartingEvent>
	{
		private readonly ILogger<AnalyzeStartingEventHandler> _logger;

		public AnalyzeStartingEventHandler(ILogger<AnalyzeStartingEventHandler> logger)
		{
			_logger = logger;
		}

		public Task Handle(AnalyzeStartingEvent @event)
		{
			throw new NotImplementedException();
		}
	}
}
