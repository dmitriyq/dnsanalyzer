using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Dns.Contracts.Messages;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Analyzer.Services.Implementation
{
	public class BatchingSuspectService
	{
		private readonly Timer _timer;
		private readonly ILogger<BatchingSuspectService> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly TimeSpan _timeOut;
		private readonly ConcurrentBag<SuspectDomainFoundMessage> _items;

		public BatchingSuspectService(ILogger<BatchingSuspectService> logger, TimeSpan timeout, IMessageQueue messageQueue)
		{
			_logger = logger;
			_timeOut = timeout;
			_messageQueue = messageQueue;
			_items = new ConcurrentBag<SuspectDomainFoundMessage>();
			_timer = new Timer(_timeOut.TotalMilliseconds)
			{
				AutoReset = true
			};
			_timer.Elapsed += (s, e) => {
				_logger.LogInformation($"For {_timeOut} interval was collected {_items.Count} messages");
				if (_items.Count > 0)
				{
					var messages = _items.ToArray();
					_messageQueue.Publish(new SuspectBatchCreatedMessage(messages));
					_items.Clear();
				}
			};
			_timer.Start();
		}

		public void Add(SuspectDomainFoundMessage message)
		{
			if (!message.Domain.IsBlank() && message.IpAddresses.Count > 0)
			{
				_items.Add(message);
			}
		}
	}
}
