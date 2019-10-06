using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Protobuf;

namespace Dns.Resolver.Whitelist.Services
{
	public interface IWhitelistService
	{
		Task AddOrUpdateDomainsAsync(IEnumerable<ResolvedDomain> domains);
		Task<ResolvedDomain?> GetDomainAsync(string domain);
		Task<IEnumerable<ResolvedDomain>> GetDomainsAsync();
		Task<IEnumerable<string>> GetDomainNamesAsync();
	}
}
