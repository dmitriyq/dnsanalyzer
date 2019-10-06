using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dns.Contracts.Messages;
using Dns.Contracts.Protobuf;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Whitelist.Services
{
	public class WhitelistUpdateService : IWhitelistUpdateService
	{
		private readonly IDomainLookupService _domainLookup;
		private readonly IWhitelistService _whitelist;
		private readonly ILogger<WhitelistUpdateService> _logger;
		private readonly IMessageQueue _messageQueue;
		private readonly TimeSpan _refreshInterval;

		public WhitelistUpdateService(IDomainLookupService domainLookup, IWhitelistService whitelist,
			ILogger<WhitelistUpdateService> logger, IMessageQueue messageQueue, TimeSpan refreshInterval)
		{
			_domainLookup = domainLookup;
			_whitelist = whitelist;
			_logger = logger;
			_messageQueue = messageQueue;
			_refreshInterval = refreshInterval;
		}

		public async Task RunJobAsync()
		{
			_logger.LogInformation($"Update job has scheduled to executing.");
			while (true)
			{
				try
				{
					_logger.LogInformation($"Job starting.");
					var domainsToResolve = await _whitelist.GetDomainNamesAsync().ConfigureAwait(false);
					var resolveResult = await _domainLookup.GetIpAddressesAsync(domainsToResolve).ConfigureAwait(false);

					var onlySuccessed = resolveResult.Where(x => _domainLookup.IsSuccessResolve(x));

					var resolvedDomains = onlySuccessed.Select(x => new ResolvedDomain(x.domain, x.ips.ToHashSet()));
					await _whitelist.AddOrUpdateDomainsAsync(resolvedDomains).ConfigureAwait(false);
					var message = new WhiteListUpdatedMessage(resolvedDomains.ToArray().ProtoSerialize());
					await _messageQueue.PublishAsync(message).ConfigureAwait(false);
					_logger.LogInformation($"Job completed.");
					await Task.Delay(_refreshInterval).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, ex.Message);
				}
			}
		}
	}
}
