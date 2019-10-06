using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace Dns.Resolver.Analyzer.Services.Implementation
{
	public class CacheService : ICacheService
	{
		private readonly IDistributedCache _distributedCache;
		private readonly IMemoryCache _memoryCache;
		private readonly IDatabase _redisDb;

		private readonly (string ips, string subnets) _keys;

		public CacheService(IDistributedCache distributedCache, IMemoryCache memoryCache, ConnectionMultiplexer redis,
			(string ips, string subnets) keys)
		{
			_distributedCache = distributedCache;
			_memoryCache = memoryCache;
			_redisDb = redis.GetDatabase();
			_keys = keys;
		}

		public Task<HashSet<string>> GetVigruzkiIps() => GetHashSetFromCache(_keys.ips);

		public Task<HashSet<string>> GetVigruzkiSubnets() => GetHashSetFromCache(_keys.subnets);

		private async Task<HashSet<string>> GetHashSetFromCache(string key)
		{
			if (!_memoryCache.TryGetValue(key, out HashSet<string> memoryData))
			{
				var distrCache = await _distributedCache.GetAsync(key).ConfigureAwait(false);
				if (distrCache == null)
				{
					var notCachedData = await _redisDb.SetMembersAsync(key).ConfigureAwait(false);
					var hashSet = notCachedData.ToStringArray().ToHashSet();

					distrCache = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, hashSet));
					await _distributedCache.SetAsync(key, distrCache, new DistributedCacheEntryOptions()
						.SetAbsoluteExpiration(TimeSpan.FromMinutes(3))).ConfigureAwait(false);

					memoryData = hashSet;
				}
				else
				{
					memoryData = Encoding.UTF8.GetString(distrCache).Split(Environment.NewLine).ToHashSet();
				}
				_memoryCache.Set(key, memoryData,
					new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
			}
			return memoryData;
		}
	}
}
