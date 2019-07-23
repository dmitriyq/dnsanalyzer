using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Models;
using Dns.Library.Models;
using Microsoft.Extensions.Logging;

namespace Dns.Analyzer.Services
{
	public class SuspectDomainSevice
	{
		private readonly ILogger<SuspectDomainSevice> _logger;
		private readonly DnsDbContext _dbContext;

		public SuspectDomainSevice(ILogger<SuspectDomainSevice> logger, DnsDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task UpdateSuspectDomains(IEnumerable<ResolvedDomain> domains)
		{
			_dbContext.RemoveRange(_dbContext.SuspectDomains);
			var newSuspects = domains.SelectMany(x => x.IPAddresses, (domain, ipaddress) => new SuspectDomains
			{
				Domain = domain.Name,
				Ip = ipaddress
			}).ToList();
			for (int i = 0; i < newSuspects.Count; i++)
			{
				newSuspects[i].Id = i;
			}
			_dbContext.SuspectDomains.AddRange(newSuspects);
			await _dbContext.SaveChangesAsync();
			_logger.LogInformation($"Suspect domains ({newSuspects.Count}) updated");
		}
	}
}
