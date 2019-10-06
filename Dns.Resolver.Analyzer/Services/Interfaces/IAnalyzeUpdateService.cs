using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IAnalyzeUpdateService
	{
		Task RunJobAsync();
	}
}
