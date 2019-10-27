using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Dns.Resolver.Analyzer.Services.Implementation;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Analyzer.Messages
{
	public class SuspectDomainFoundMessageHandler : IAmqpMessageHandler<SuspectDomainFoundMessage>
	{
		private readonly BatchingSuspectService _batchingAttack;

		public SuspectDomainFoundMessageHandler(BatchingSuspectService batchingAttack)
		{
			_batchingAttack = batchingAttack;
		}

		public Task Handle(SuspectDomainFoundMessage message)
		{
			_batchingAttack.Add(message);
			return Task.CompletedTask;
		}
	}
}
