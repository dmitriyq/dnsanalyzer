using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Aggregator.Messages;

namespace Dns.Resolver.Aggregator.Services
{
	public interface IDomainAggregatorService
	{
		Task AddDomainAsync(DomainResolvedMessage domain);

		Task StoreDomainsAsync();

		void NotifyCompletion();
	}
}
