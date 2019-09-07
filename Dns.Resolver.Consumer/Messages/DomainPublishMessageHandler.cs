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
	public class DomainPublishMessageHandler : IMessageQueueHandler<DomainPublishMessage>
	{
		private readonly IDomainLookupService _domainLookup;
		private readonly IMessageQueue _messageQueue;
		private readonly string _resolvedQueue;

		public DomainPublishMessageHandler(IDomainLookupService domainLookup, IMessageQueue messageQueue, string resolvedQueue)
		{
			_domainLookup = domainLookup;
			_messageQueue = messageQueue;
			_resolvedQueue = resolvedQueue;
		}

		public async Task<bool> Handle(DomainPublishMessage message)
		{
			try
			{
				var ips = await _domainLookup.GetIpAddressesAsync(message.Domain).ConfigureAwait(false);
				if (ips.Count == 0)
				{
					return true;
				}
				try
				{
					var resolvedMessage = new DomainResolvedMessage(message.Domain, message.DomainType, ips, message.TraceId);
					_messageQueue.Enqueue(resolvedMessage, _resolvedQueue);
				}
				catch 
				{
					return false;
				}
				
			}
			catch {	}

			return true;
		}
	}
}
