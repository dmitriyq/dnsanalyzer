using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Whitelist.Services
{
	public interface IWhitelistUpdateService
	{
		Task RunJobAsync();
	}
}
