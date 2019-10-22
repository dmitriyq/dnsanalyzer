using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IAnalyzeService
	{
		Task<AttackFoundMessage[]> ExcludeAsync(AttackFoundMessage[] attacks);
		Task<IEnumerable<int>> UpdateAttackAsync(AttackFoundMessage[] attacks);
		Task<IEnumerable<int>> UpdateAttackGroupAsync();
		Task<IEnumerable<int>> CheckForExpiredAttacksAsync();
	}
}
