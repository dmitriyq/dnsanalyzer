using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DNS.Protocol;

namespace Dns.Resolver.Consumer.Services
{
	public interface IDomainLookupService
	{
		Task<(ISet<string> ips, ResponseCode code)> GetIpAddressesAsync(string domain);
	}
}
