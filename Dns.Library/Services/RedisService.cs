using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Library.Models;
using Dns.Library.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Library.Services
{
	public class RedisService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly RedisOptions _options = new RedisOptions();

		public RedisService(Action<RedisOptions> opts)
		{
			opts.Invoke(_options);
			_redis = ConnectionMultiplexer.Connect(_options.ConnectionString);
		}

		public async Task<HashSet<string>> GetBlackDomains()
		{
			var db = _redis.GetDatabase();
			var domains = await db.SetMembersAsync(RedisKeys.BLACK_DOMAINS);
			return domains.ToStringArray().ToHashSet();
		}

		public async Task SaveResolvedDomains(string key, IEnumerable<ResolvedDomain> domains)
		{
			var redisArray = domains.Select(x => x.ProtoSerialize())
				.Select(x => (RedisValue)x)
				.ToArray();
			var db = _redis.GetDatabase();
			var transaction = db.CreateTransaction();
			var deleteResult = transaction.KeyDeleteAsync(key);
			var addResult = transaction.SetAddAsync(key, redisArray);
			var transactionResult = await transaction.ExecuteAsync();
		}
	}
	public class RedisKeys
	{
		public const string BLACK_DOMAINS = "Vigruzki_Domains";
		public const string BLACK_DOMAINS_RESOLVED = "DNS_Resolver_Black";
		public const string WHITE_DOMAINS_RESOLVED = "DNS_Resolver_White";
	}
	public class RedisOptions
	{
		public string ConnectionString { get; set; }
	}
}
