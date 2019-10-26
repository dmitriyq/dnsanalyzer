using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Dns.DAL.Models;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Analyzer.Messages
{
	public class SuspectBatchCreatedMessageHandler : IAmqpMessageHandler<SuspectBatchCreatedMessage>
	{
		private readonly DnsDbContext _dnsDbContext;
		private readonly ILogger<SuspectBatchCreatedMessageHandler> _logger;

		public SuspectBatchCreatedMessageHandler(DnsDbContext dnsDbContext, ILogger<SuspectBatchCreatedMessageHandler> logger)
		{
			_dnsDbContext = dnsDbContext;
			_logger = logger;
		}

		public async Task Handle(SuspectBatchCreatedMessage message)
		{
			_dnsDbContext.RemoveRange(_dnsDbContext.SuspectDomains);
			_logger.LogInformation($"Suspect domains - {string.Join('\n', message.SuspectDomainMessages.Select(x => $"[{x.Domain} - {string.Join(',', x.IpAddresses)}]"))}");
			var newSuspects = message.SuspectDomainMessages.SelectMany(x => x.IpAddresses, (domain, ipaddress) => new SuspectDomains
			{
				Domain = domain.Domain,
				Ip = ipaddress
			}).Distinct().ToList();
			for (int i = 0; i < newSuspects.Count; i++)
			{
				newSuspects[i].Id = i;
			}
			_dnsDbContext.SuspectDomains.AddRange(newSuspects);
			await _dnsDbContext.SaveChangesAsync().ConfigureAwait(false);
			_logger.LogInformation($"Suspect domains ({newSuspects.Count}) updated");
		}
	}
}
