using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Protobuf;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Blacklist.Services.Implementation
{
	public class CacheService : ICacheService
	{
		private readonly IDistributedCache _distributedCache;
		private readonly IMemoryCache _memoryCache;
		private readonly IDatabase _redis;
		private readonly ILogger<CacheService> _logger;
		private readonly string _whiteListKey;
		private readonly string _blackListKey;

		public CacheService(IDistributedCache distributedCache, IMemoryCache memoryCache, ConnectionMultiplexer redis,
			ILogger<CacheService> logger, string whiteListKey, string blackListKey)
		{
			_distributedCache = distributedCache;
			_memoryCache = memoryCache;
			_redis = redis.GetDatabase();
			_logger = logger;
			_whiteListKey = whiteListKey;
			_blackListKey = blackListKey;
		}

		public async Task<ResolvedDomain[]> GetWhiteListAsync()
		{
			if (!_memoryCache.TryGetValue(_whiteListKey, out ResolvedDomain[] domains))
			{
				var distCacheValue = await _distributedCache.GetAsync(_whiteListKey).ConfigureAwait(false);
				if (distCacheValue != null)
				{
					domains = await _distributedCache.GetAsync(_whiteListKey)
					.ContinueWith(t => t.Result.ProtoDeserialize<ResolvedDomain[]>())
					.ConfigureAwait(false);
					var cacheOpts = new MemoryCacheEntryOptions()
						.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
					_memoryCache.Set(_whiteListKey, domains, cacheOpts);
				}
			}
			return domains;
		}

		public Task UpdateWhiteListAsync(ResolvedDomain[] domains) =>
			_distributedCache.SetAsync(_whiteListKey, domains.ProtoSerialize())
				.ContinueWith(_ => _memoryCache.Set(_whiteListKey, domains));

		public async Task<ISet<string>> GetBlackDomainsAsync()
		{
			var redisDomains = await _redis.SetMembersAsync(_blackListKey).ConfigureAwait(false);
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
	}
}
