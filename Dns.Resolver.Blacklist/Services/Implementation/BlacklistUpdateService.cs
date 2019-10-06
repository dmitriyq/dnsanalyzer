using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Blacklist.Services.Implementation
{
	public class BlacklistUpdateService : IBlacklistUpdateService
	{
		private readonly IDistributedCache _distributedCache;
		private readonly TimeSpan _updateInterval;
		private readonly IMessageQueue _messageQueue;
		private readonly ICacheService _cacheService;
		private readonly ILogger<BlacklistUpdateService> _logger;

		public BlacklistUpdateService(IDistributedCache distributedCache, IMessageQueue messageQueue, ICacheService cacheService,
			ILogger<BlacklistUpdateService> logger, TimeSpan updateInterval)
		{
			_distributedCache = distributedCache;
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
					var hostname = System.Net.Dns.GetHostName();
					var rnd = new Random();
					var interval = rnd.Next(100, 5000);
					await Task.Delay(interval)
						.ContinueWith(_ => _distributedCache.SetStringAsync("random_leader", hostname))
						.ContinueWith(_ => Task.Delay(5000 - interval))
						.ConfigureAwait(false);
					var leader = await _distributedCache.GetStringAsync("random_leader").ConfigureAwait(false);
					_logger.LogInformation($"Leader selected - {leader}");
					if (leader == hostname)
					{
						foreach (var domain in await _cacheService.GetBlackDomainsAsync().ConfigureAwait(false))
						{
							await _messageQueue.PublishAsync(new BlackDomainResolveNeededMessage(domain)).ConfigureAwait(false);
						}
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
