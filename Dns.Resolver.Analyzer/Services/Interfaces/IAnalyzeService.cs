using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IAnalyzeService
	{
		Task<bool> IsExcludedAsync(AttackFoundMessage attack);
		Task<int?> UpdateAttackAsync(AttackFoundMessage attack);
		Task<IEnumerable<int>> UpdateAttackGroupAsync(AttackFoundMessage attack);
		Task<IEnumerable<int>> CheckForExpiredAttacksAsync();
	}
}
