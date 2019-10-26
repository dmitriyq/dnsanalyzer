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

		public AnalyzeUpdateService(ILogger<AnalyzeUpdateService> logger, TimeSpan refreshInterval, IAnalyzeService analyzeService,
			INotifyService notifyService)
		{
			_logger = logger;
			_refreshInterval = refreshInterval;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
		}

		public async Task RunJobAsync()
		{
			var startDate = DateTime.Now;
			var firstStart = startDate.RoundUp(_refreshInterval);
			await Task.Delay(firstStart - startDate).ConfigureAwait(false);
			while (true)
			{
				try
				{
					var updatedGroupIds = _analyzeService.CheckForExpiredAttacks();
					if (updatedGroupIds.Any())
					{
						var msg = _notifyService.BuildGroupMessage(string.Empty, updatedGroupIds.ToArray());
						await _notifyService.SendAsync(msg).ConfigureAwait(false);
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
