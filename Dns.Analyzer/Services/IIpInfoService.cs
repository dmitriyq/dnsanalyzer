using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL.Models;

namespace Dns.Analyzer.Services
{
	public interface IIpInfoService
	{
		IpInfo? GetInfo(string ip);
		Task UpdateIpInfoAsync(bool onlyUnresolved);
	}
}
