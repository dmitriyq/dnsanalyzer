using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Consumer.Services
{
	public interface IDomainLookupService
	{
		Task<ISet<string>> GetIpAddressesAsync(string domain);
	}
}
