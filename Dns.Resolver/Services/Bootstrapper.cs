using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.Library.Models;
using Dns.Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Services
{
	public class Bootstrapper
	{
		private readonly ILogger<Bootstrapper> _logger;
		private readonly ResolveService _resolve;
		private readonly DnsReadOnlyDbContext _dnsReadOnly;
		private readonly RedisService _redis;

		public Bootstrapper(
			ILogger<Bootstrapper> logger, 
			ResolveService resolve, 
			DnsReadOnlyDbContext dnsReadOnly, 
			RedisService redisService)
		{
			_logger = logger;
			_resolve = resolve;
			_dnsReadOnly = dnsReadOnly;
			_redis = redisService;
		}

		public async Task ResolveDomains()
		{
			_logger.LogInformation("1. Starting resolve domains");

			_logger.LogInformation("2. Getting domains to resolve");
			var whiteDomains = await GetWhiteDomains();
			var blackDomains = await GetBlackDomains();

			_logger.LogInformation("3. Resolving domains");
			var resolvedWhiteDomains = await _resolve.ResolveDomainsWithRetry(whiteDomains, 3);
			var resolvedBlackDomains = await _resolve.ResolveDomainsWithRetry(blackDomains, 3);

			_logger.LogInformation("4. Saving resolve result");
			await _redis.SaveResolvedDomains(RedisKeys.WHITE_DOMAINS_RESOLVED, resolvedWhiteDomains);
			await _redis.SaveResolvedDomains(RedisKeys.BLACK_DOMAINS_RESOLVED, resolvedBlackDomains);
			_logger.LogInformation("5. Finishing resolve domains");
		}

		

		private async Task<List<string>> GetWhiteDomains() =>
			await _dnsReadOnly.WhiteDomains.Select(x => x.Domain).ToListAsync();
		private async Task<List<string>> GetBlackDomains() =>
			(await _redis.GetBlackDomains()).ToList();
	}
}
