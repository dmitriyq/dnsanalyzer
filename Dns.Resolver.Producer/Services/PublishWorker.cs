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
	public class PublishWorker
	{
		private readonly ILogger<PublishWorker> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly IDomainService _domainService;
		private readonly TimeSpan _timeOut;
		private readonly string _queueName;
		private readonly string _healthQueue;

		private readonly int _producerLimit;
		private readonly int _producerLimitTimeout;

		public PublishWorker(ILogger<PublishWorker> logger, IMessageQueue messageQueue, IDomainService domainService,
			string queueName, string healthQueue, int producerLimit, int producerLimitTimeout)
		{
			_logger = logger;
			_messageQueue = messageQueue;
			_domainService = domainService;
			_timeOut = TimeSpan.FromSeconds(double.Parse(EnvironmentExtensions.GetVariable(Program.RESOLVER_PUBLISHER_DELAY_SEC)));
			_queueName = queueName;
			_healthQueue = healthQueue;
			_producerLimit = producerLimit;
			_producerLimitTimeout = producerLimitTimeout;
		}

		public async Task RunJob()
		{
			_logger.LogInformation($"PublishWorker is starting.");

			while (true)
			{
				_logger.LogInformation($"PublishWorker background task executing.");

				await PublishDomains().ConfigureAwait(false);

				_logger.LogInformation($"PublishWorker background task executed successfully.");
				_messageQueue.Enqueue(new DnsAnalyzerHealthCheckMessage("Dns.Resolver.Producer", "Отправлены домены на резолв"), _healthQueue);
				await Task.Delay(_timeOut).ConfigureAwait(false);
			}
			throw new Exception();
		}

		private async Task PublishDomains()
		{
			var blackDomains = await _domainService.GetBlackDomainsAsync().ConfigureAwait(false);
			var whiteDomains = await _domainService.GetWhiteDomainsAsync().ConfigureAwait(false);
			var traceId = Guid.NewGuid();

			int sendedCount = 0;
			foreach (var blackDomain in blackDomains)
			{
				_messageQueue.Enqueue(new DomainPublishMessage(blackDomain, 1, traceId), _queueName);
				sendedCount++;
				if (sendedCount > _producerLimit)
				{
					await Task.Delay(_producerLimitTimeout);
					sendedCount = 0;
				}
			}
			foreach (var whiteDomain in whiteDomains)
			{
				_messageQueue.Enqueue(new DomainPublishMessage(whiteDomain, 2, traceId), _queueName);
				sendedCount++;
				if (sendedCount > _producerLimit)
				{
					await Task.Delay(_producerLimitTimeout);
					sendedCount = 0;
				}
			}
		}
	}
}
