using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Producer.Services
{
	public interface IDomainService
	{
		Task<ISet<string>> GetWhiteDomainsAsync();
		Task<ISet<string>> GetBlackDomainsAsync();
	}
}
