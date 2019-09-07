using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Analyzer.Models;
using Dns.Contracts.Protobuf;

namespace Dns.Analyzer.Services
{
	public interface IAnalyzeService
	{
		IEnumerable<AttackModel> FindIntersection(IEnumerable<ResolvedDomain> blackDomains, IEnumerable<ResolvedDomain> whiteDomains);
		Task<IEnumerable<AttackModel>> ExcludeDomainsAsync(IEnumerable<AttackModel> attacks);
		Task<IEnumerable<int>> UpdateAttacksAsync(IEnumerable<AttackModel> attacks, ISet<string> vigruzkiIps, ISet<string> vigruzkiSubnets);
	}
}
