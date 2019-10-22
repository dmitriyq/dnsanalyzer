using System;
using System.Collections.Generic;
using System.Text;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IBatchingAttackService
	{
		void Add(AttackFoundMessage message);
	}
}
