using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dns.Library;
using Dns.Resolver.Publisher.Events;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Publisher.Services
{
	public class PublishWorker : BackgroundService
	{
		private readonly ILogger<PublishWorker> _logger;
		private readonly IEventBus _eventBus;
		private readonly IDomainService _domainService;
		private readonly TimeSpan _timeOut;
		private readonly IHostApplicationLifetime _applicationLifetime;

		public PublishWorker(ILogger<PublishWorker> logger, IEventBus eventBus, IDomainService domainService, IHostApplicationLifetime applicationLifetime)
		{
			_logger = logger;
			_eventBus = eventBus;
			_domainService = domainService;
			_applicationLifetime = applicationLifetime;
			_timeOut = TimeSpan.FromSeconds(double.Parse(EnvironmentExtensions.GetVariable(EnvVars.RESOLVER_PUBLISHER_DELAY_SEC)));

			_applicationLifetime.ApplicationStopping.Register(async () => await StopAsync(new CancellationToken()).ConfigureAwait(false));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"PublishWorker is starting.");

			stoppingToken.Register(() => _logger.LogInformation($"PublishWorker background task is stopping."));

			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation($"PublishWorker background task executing.");

				await PublishDomains().ConfigureAwait(false);

				_logger.LogInformation($"PublishWorker background task executed successfully.");

				await Task.Delay(_timeOut, stoppingToken).ConfigureAwait(false);
			}
			_logger.LogInformation("PublishWorker background task is stopping.");

			await Task.CompletedTask.ConfigureAwait(false);
		}

		private async Task PublishDomains()
		{
			var blackDomains = await _domainService.GetBlackDomainsAsync().ConfigureAwait(false);
			var whiteDomains = await _domainService.GetWhiteDomainsAsync().ConfigureAwait(false);

			foreach (var blackDomain in blackDomains)
			{
				_eventBus.Publish(new DomainPublisherEvent(blackDomain, 1));
			}
			foreach (var whiteDomain in whiteDomains)
			{
				_eventBus.Publish(new DomainPublisherEvent(whiteDomain, 2));
			}
		}
	}
}
