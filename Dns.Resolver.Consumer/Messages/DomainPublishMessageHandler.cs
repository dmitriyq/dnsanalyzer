using System;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Consumer.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Consumer.Messages
{
	public class DomainPublishMessageHandler : IAmqpMessageHandler<DomainPublishMessage>
	{
		private readonly IDomainLookupService _domainLookup;
		private readonly ILogger<DomainPublishMessageHandler> _logger;
		private readonly IMessageQueue _messageQueue;

		public DomainPublishMessageHandler(IDomainLookupService domainLookup, ILogger<DomainPublishMessageHandler> logger,
			IMessageQueue messageQueue)
		{
			_domainLookup = domainLookup;
			_messageQueue = messageQueue;
			_logger = logger;
		}

		public async Task Handle(DomainPublishMessage message)
		{
			try
			{
				var (ips, code) = await _domainLookup.GetIpAddressesAsync(message.Domain).ConfigureAwait(false);
				if (code == DNS.Protocol.ResponseCode.NoError && ips.Count > 0)
				{
					try
					{
						var resolvedMessage = new DomainResolvedMessage(message.Domain, message.DomainType, ips, message.TraceId);
						await _messageQueue.PublishAsync(resolvedMessage).ConfigureAwait(false);
					}
					catch
					{
						return;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
			}
		}
	}
}
