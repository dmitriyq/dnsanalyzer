using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Blacklist.Services.Implementation
{
	public class BlacklistUpdateService : IBlacklistUpdateService
	{
		private readonly TimeSpan _updateInterval;
		private readonly IMessageQueue _messageQueue;
		private readonly ICacheService _cacheService;
		private readonly ILogger<BlacklistUpdateService> _logger;

		public BlacklistUpdateService(IMessageQueue messageQueue, ICacheService cacheService,
			ILogger<BlacklistUpdateService> logger, TimeSpan updateInterval)
		{
			_messageQueue = messageQueue;
			_cacheService = cacheService;
			_logger = logger;
			_updateInterval = updateInterval;
		}

		public async Task RunJobAsync()
		{
			_logger.LogInformation($"Update job has scheduled to executing.");
			var startDate = DateTime.Now;
			var firstStart = startDate.RoundUp(_updateInterval);
			_logger.LogInformation($"First start at {firstStart}");
			await Task.Delay(firstStart - startDate).ConfigureAwait(false);
			while (true)
			{
				try
				{
					_logger.LogInformation($"Job starting.");
					foreach (var domain in await _cacheService.GetBlackDomainsAsync().ConfigureAwait(false))
					{
						await _messageQueue.PublishAsync(new BlackDomainResolveNeededMessage(domain)).ConfigureAwait(false);
					}
					_logger.LogInformation($"Job completed.");
					var now = DateTime.Now;
					await Task.Delay(now.RoundUp(_updateInterval) - now).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, ex.Message);
				}
			}
		}
	}
}
