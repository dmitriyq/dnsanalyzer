using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Grfc.Library.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Producer.Services
{
	public class DomainService : IDomainService
	{
		private readonly DnsDbContext _dnsDb;
		private readonly IDatabase _redisDb;
		private readonly ILogger<DomainService> _logger;

		public DomainService(DnsDbContext dnsDb, ConnectionMultiplexer redis, ILogger<DomainService> logger)
		{
			_dnsDb = dnsDb;
			_redisDb = redis.GetDatabase();
			_logger = logger;
		}

		public async Task<ISet<string>> GetBlackDomainsAsync()
		{
			var redisKey = EnvironmentExtensions.GetVariable(Program.REDIS_BLACK_DOMAINS);
			_logger.LogInformation($"Trying get black domains from redis server by {redisKey} key");

			var redisDomains = await _redisDb.SetMembersAsync(redisKey).ConfigureAwait(false);
			var stringArrays = redisDomains.ToStringArray();

			var dottedDomains = stringArrays.Where(x => x.EndsWith(".")).ToHashSet();
			var maskedDomains = stringArrays.Where(x => x.StartsWith("*.")).ToHashSet();

			var normalizedDotted = dottedDomains.Select(x => x[0..^1]);
			var normalizedMasked = maskedDomains.Select(x => x.Remove(0, 2));

			var allDomains = stringArrays.Except(dottedDomains).Except(maskedDomains).ToList();
			allDomains.AddRange(normalizedDotted);
			allDomains.AddRange(normalizedMasked);

			var blackDomains = allDomains.ToHashSet();
			_logger.LogInformation($"Found {blackDomains.Count} black domains");
			return blackDomains;
		}

		public async Task<ISet<string>> GetWhiteDomainsAsync()
		{
			_logger.LogInformation("Trying get white domains from postgres");
			var domains = await _dnsDb.WhiteDomains.Select(x => x.Domain).Distinct().ToListAsync().ConfigureAwait(false);
			var whiteDomains = domains.ToHashSet();
			_logger.LogInformation($"Found {whiteDomains.Count} white domains");
			return whiteDomains;
		}
	}
}
