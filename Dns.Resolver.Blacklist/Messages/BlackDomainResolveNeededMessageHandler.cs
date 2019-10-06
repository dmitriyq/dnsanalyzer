using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.EventBus.Abstractions;

namespace Dns.Resolver.Blacklist.Messages
{
	public class BlackDomainResolveNeededMessageHandler : IAmqpMessageHandler<BlackDomainResolveNeededMessage>
	{
		private readonly IDomainLookupService _domainLookup;
		private readonly ICacheService _cacheService;
		private readonly IMessageQueue _messageQueue;

		public BlackDomainResolveNeededMessageHandler(IDomainLookupService domainLookup, ICacheService cacheService,
			IMessageQueue messageQueue)
		{
			_domainLookup = domainLookup;
			_cacheService = cacheService;
			_messageQueue = messageQueue;
		}

		public async Task Handle(BlackDomainResolveNeededMessage message)
		{
			var resolve = await _domainLookup.GetIpAddressesAsync(message.Domain).ConfigureAwait(false);
			if (_domainLookup.IsSuccessResolve(resolve))
			{
				var whiteDomains = await _cacheService.GetWhiteListAsync().ConfigureAwait(false);
				if (whiteDomains != null)
				{
					foreach (var ip in resolve.ips)
					{
						if (whiteDomains.Any(x => x.IPAddresses.Contains(ip)))
						{
							var ipIntersection = whiteDomains.Where(x => x.IPAddresses.Contains(ip));
							foreach (var intersection in ipIntersection)
							{
								var msg = new AttackFoundMessage(message.Domain, intersection.Name, ip);
								await _messageQueue.PublishAsync(msg).ConfigureAwait(false);
							}
						}
					}
				}
			}
		}
	}
}
