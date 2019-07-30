using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.Library;
using Dns.Library.Models;
using Dns.Library.Services;
using Grfc.Library.Common.Extensions;
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
			var whiteDomains = await GetWhiteDomains();
			var blackDomains = await GetBlackDomains();

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var resolvedWhiteDomains = await _resolve.ResolveDomainsWithRetry(whiteDomains, 3);
			stopWatch.Stop();
			_logger.LogWarning($"[RESOLVING, PARALLELISM - {EnvironmentExtensions.GetVariable(EnvVars.RESOLVER_MAX_DEGREE_OF_PARALLELISM)}, BUFFER - {EnvironmentExtensions.GetVariable(EnvVars.RESOLVER_BUFFER_BLOCK_SIZE)}] {resolvedWhiteDomains.Count()} domains took {stopWatch.Elapsed}");
			stopWatch.Restart();
			var resolvedBlackDomains = await _resolve.ResolveDomainsWithRetry(blackDomains, 3);
			_logger.LogWarning($"[RESOLVING, PARALLELISM - {EnvironmentExtensions.GetVariable(EnvVars.RESOLVER_MAX_DEGREE_OF_PARALLELISM)}, BUFFER - {EnvironmentExtensions.GetVariable(EnvVars.RESOLVER_BUFFER_BLOCK_SIZE)}] {resolvedBlackDomains.Count()} domains took {stopWatch.Elapsed}");
			stopWatch.Stop();

			await _redis.SaveResolvedDomains(RedisKeys.WHITE_DOMAINS_RESOLVED, resolvedWhiteDomains);
			await _redis.SaveResolvedDomains(RedisKeys.BLACK_DOMAINS_RESOLVED, resolvedBlackDomains);
		}

		public async Task NotifyCompletion()
		{
			await _redis.Publish(RedisKeys.RESOLVE_COMPLETE_CHANNEL, DateTimeOffset.UtcNow.ToString("o"));
		}

		private async Task<List<string>> GetWhiteDomains() =>
			await _dnsReadOnly.WhiteDomains.Select(x => x.Domain).ToListAsync();
		private async Task<List<string>> GetBlackDomains()
		{
			var domains = await _redis.GetStringSetMembers(RedisKeys.BLACK_DOMAINS);

			var dottedDomains = domains.Where(x => x.EndsWith(".")).ToHashSet();
			var maskedDomains = domains.Where(x => x.StartsWith("*.")).ToHashSet();
			
			var normalizedDotted = dottedDomains.Select(x => x.Substring(0, x.Length - 1));
			var normalizedMasked = maskedDomains.Select(x => x.Remove(0, 2));

			var allDomains = domains.Except(dottedDomains).Except(maskedDomains).ToList();
			allDomains.AddRange(normalizedDotted);
			allDomains.AddRange(normalizedMasked);
			return allDomains.Distinct().ToList();
		}
	}
}
