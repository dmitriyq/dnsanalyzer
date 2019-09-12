using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Protobuf;
using Dns.DAL;
using Dns.DAL.Models;
using Microsoft.Extensions.Logging;

namespace Dns.Analyzer.Services
{
	public class SuspectDomainSevice
	{
		private readonly ILogger<SuspectDomainSevice> _logger;
		private readonly DnsDbContext _dbContext;
		private readonly int _ipCount;

		public SuspectDomainSevice(ILogger<SuspectDomainSevice> logger, DnsDbContext dbContext, int suspectIpCountTreshhold)
		{
			_logger = logger;
			_dbContext = dbContext;
			_ipCount = suspectIpCountTreshhold;
		}

		public async Task UpdateSuspectDomains(IEnumerable<ResolvedDomain> domains)
		{
			var suspectDomains = domains.Where(x => x.IPAddresses.Count > _ipCount).AsEnumerable();
			_dbContext.RemoveRange(_dbContext.SuspectDomains);
			var newSuspects = suspectDomains.SelectMany(x => x.IPAddresses, (domain, ipaddress) => new SuspectDomains
			{
				Domain = domain.Name,
				Ip = ipaddress
			}).ToList();
			for (int i = 0; i < newSuspects.Count; i++)
			{
				newSuspects[i].Id = i;
			}
			_dbContext.SuspectDomains.AddRange(newSuspects);
			await _dbContext.SaveChangesAsync().ConfigureAwait(false);
			_logger.LogInformation($"Suspect domains ({newSuspects.Count}) updated");
		}
	}
}