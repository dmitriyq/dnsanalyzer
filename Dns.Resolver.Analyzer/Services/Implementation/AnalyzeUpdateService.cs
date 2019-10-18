using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Services;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Analyzer.Services.Implementation
{
	public class AnalyzeUpdateService : IAnalyzeUpdateService
	{
		private readonly ILogger<AnalyzeUpdateService> _logger;
		private readonly TimeSpan _refreshInterval;
		private readonly IAnalyzeService _analyzeService;
		private readonly INotifyService _notifyService;
		private readonly IDatabase _redis;
		private readonly string _notifyChannel;

		public AnalyzeUpdateService(ILogger<AnalyzeUpdateService> logger, TimeSpan refreshInterval, IAnalyzeService analyzeService,
			INotifyService notifyService, ConnectionMultiplexer redis, string notifyChannel)
		{
			_logger = logger;
			_refreshInterval = refreshInterval;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_redis = redis.GetDatabase();
			_notifyChannel = notifyChannel;
		}

		public async Task RunJobAsync()
		{
			_logger.LogInformation($"Update job has scheduled to executing.");
			var startDate = DateTime.Now;
			var firstStart = startDate.RoundUp(_refreshInterval);
			_logger.LogInformation($"First start at {firstStart}");
			await Task.Delay(firstStart - startDate).ConfigureAwait(false);
			while (true)
			{
				try
				{
					_logger.LogInformation($"Job starting.");
					var updatedAttackIds = await _analyzeService.CheckForExpiredAttacksAsync().ConfigureAwait(false);
					if (updatedAttackIds.Any())
					{
						var msg = _notifyService.BuildAttackMessage(string.Empty, updatedAttackIds.ToArray());
						await _redis.PublishAsync(_notifyChannel, msg.ProtoSerialize()).ConfigureAwait(false);
					}
					_logger.LogInformation($"Job completed.");
					var now = DateTime.Now;
					await Task.Delay(now.RoundUp(_refreshInterval) - now).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, ex.Message);
				}
			}
		}
	}
}
