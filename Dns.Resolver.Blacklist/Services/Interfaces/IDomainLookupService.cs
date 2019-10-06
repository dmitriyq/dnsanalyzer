using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DNS.Protocol;

namespace Dns.Resolver.Blacklist.Services.Interfaces
{
	public interface IDomainLookupService
	{
		Task<(ISet<string> ips, ResponseCode code)> GetIpAddressesAsync(string domain);

		Task<IEnumerable<(string domain, ISet<string> ips, ResponseCode code)>> GetIpAddressesAsync(IEnumerable<string> domains);

		bool IsSuccessResolve((ISet<string> ips, ResponseCode code) resolve);
		bool IsSuccessResolve((string domain, ISet<string> ips, ResponseCode code) resolve);
	}
}
