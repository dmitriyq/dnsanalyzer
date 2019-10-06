using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Protobuf;

namespace Dns.Resolver.Blacklist.Services.Interfaces
{
	public interface ICacheService
	{
		Task UpdateWhiteListAsync(ResolvedDomain[] domains);
		Task<ResolvedDomain[]> GetWhiteListAsync();
		Task<ISet<string>> GetBlackDomainsAsync();
	}
}
