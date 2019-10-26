using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IAnalyzeService
	{
		AttackFoundMessage[] Exclude(AttackFoundMessage[] attacks);
		Task<IEnumerable<int>> UpdateAttackAsync(AttackFoundMessage[] attacks);
		IEnumerable<int> UpdateAttackGroup();
		IEnumerable<int> CheckForExpiredAttacks();
	}
}
