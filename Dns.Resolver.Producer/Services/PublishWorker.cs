using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Producer.Services
{
	public class PublishWorker : BackgroundService
	{
		private readonly ILogger<PublishWorker> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly IDomainService _domainService;
		private readonly TimeSpan _timeOut;
		private readonly IHostApplicationLifetime _applicationLifetime;
		private readonly string _queueName;
		private readonly string _healthQueue;

		public PublishWorker(ILogger<PublishWorker> logger, IMessageQueue messageQueue, IDomainService domainService, 
			IHostApplicationLifetime applicationLifetime, string queueName, string healthQueue)
		{
			_logger = logger;
			_messageQueue = messageQueue;
			_domainService = domainService;
			_applicationLifetime = applicationLifetime;
			_timeOut = TimeSpan.FromSeconds(double.Parse(EnvironmentExtensions.GetVariable(Program.RESOLVER_PUBLISHER_DELAY_SEC)));
			_queueName = queueName;
			_healthQueue = healthQueue;
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
				_messageQueue.Enqueue(new DnsAnalyzerHealthCheckMessage("Dns.Resolver.Producer", "Отправлены домены на резолв"), _healthQueue);
				await Task.Delay(_timeOut, stoppingToken).ConfigureAwait(false);
			}
			_logger.LogInformation("PublishWorker background task is stopping.");

			await Task.CompletedTask.ConfigureAwait(false);
		}

		private async Task PublishDomains()
		{
			var blackDomains = await _domainService.GetBlackDomainsAsync().ConfigureAwait(false);
			var whiteDomains = await _domainService.GetWhiteDomainsAsync().ConfigureAwait(false);
			var traceId = Guid.NewGuid();
			foreach (var blackDomain in blackDomains)
			{
				_messageQueue.Enqueue(new DomainPublishMessage(blackDomain, 1, traceId), _queueName);
			}
			foreach (var whiteDomain in whiteDomains)
			{
				_messageQueue.Enqueue(new DomainPublishMessage(whiteDomain, 2, traceId), _queueName);
			}
		}
	}
}
