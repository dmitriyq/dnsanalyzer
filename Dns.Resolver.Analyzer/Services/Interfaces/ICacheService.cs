using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface ICacheService
	{
		Task<HashSet<string>> GetVigruzkiIps();
		Task<HashSet<string>> GetVigruzkiSubnets();
	}
}
