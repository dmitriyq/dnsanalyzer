using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Contracts.Services;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
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
		private readonly IMessageQueue _messageQueue;

		public AnalyzeUpdateService(ILogger<AnalyzeUpdateService> logger, TimeSpan refreshInterval, IAnalyzeService analyzeService,
			INotifyService notifyService, IMessageQueue messageQueue)
		{
			_logger = logger;
			_refreshInterval = refreshInterval;
			_analyzeService = analyzeService;
			_notifyService = notifyService;
			_messageQueue = messageQueue;
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
						var updateMessage = new UpdatedAttackMessage(new List<int>(), updatedGroupIds.ToList());
						await _messageQueue.PublishAsync(updateMessage).ConfigureAwait(false);

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
