using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dns.Resolver.Analyzer.Messages
{
	public class SuspectDomainFoundMessageHandler : IAmqpMessageHandler<SuspectDomainFoundMessage>
	{
		private readonly DnsDbContext _dnsDb;

		public SuspectDomainFoundMessageHandler(DnsDbContext dnsDb)
		{
			_dnsDb = dnsDb;
		}

		public Task Handle(SuspectDomainFoundMessage message)
		{
			throw new NotImplementedException();
		}
	}
}
