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

		public PublishWorker(ILogger<PublishWorker> logger, IMessageQueue messageQueue, IDomainService domainService)
		{
			_logger = logger;
			_messageQueue = messageQueue;
			_domainService = domainService;
			_timeOut = TimeSpan.FromSeconds(double.Parse(EnvironmentExtensions.GetVariable(Program.RESOLVER_PUBLISHER_DELAY_SEC)));
		}

		public async Task RunJob()
		{
			_logger.LogInformation($"PublishWorker is starting.");

			while (true)
			{
				try
				{
					_logger.LogInformation($"PublishWorker background task executing.");

					var guid = await PublishDomains().ConfigureAwait(false);

					_logger.LogInformation($"PublishWorker background task executed successfully with {guid}.");
					await _messageQueue.PublishAsync(new DnsAnalyzerHealthCheckMessage("Dns.Resolver.Producer", "Отправлены домены на резолв")).ConfigureAwait(false);
					await Task.Delay(_timeOut).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, ex.Message);
				}
			}
			throw new Exception();
		}

		private async Task<Guid> PublishDomains()
		{
			var blackDomains = await _domainService.GetBlackDomainsAsync().ConfigureAwait(false);
			var whiteDomains = await _domainService.GetWhiteDomainsAsync().ConfigureAwait(false);
			var traceId = Guid.NewGuid();
			foreach (var blackDomain in blackDomains)
			{
				await _messageQueue.PublishAsync(new DomainPublishMessage(blackDomain, 1, traceId)).ConfigureAwait(false);
			}
			foreach (var whiteDomain in whiteDomains)
			{
				await _messageQueue.PublishAsync(new DomainPublishMessage(whiteDomain, 2, traceId)).ConfigureAwait(false);
			}
			return traceId;
		}
	}
}
