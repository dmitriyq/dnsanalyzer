using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Blacklist.Services.Interfaces
{
	public interface IBlacklistUpdateService
	{
		Task RunJobAsync();
	}
}
