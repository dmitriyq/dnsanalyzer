using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Library.Models;
using Dns.Library.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Grfc.Library.Common.Extensions;

namespace Dns.Library.Services
{
	public class RedisService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly ILogger<RedisService> _logger;
		public RedisService(ILogger<RedisService> logger)
		{
			_logger = logger;
			_redis = ConnectionMultiplexer.Connect(EnvironmentExtensions.GetVariable(EnvVars.REDIS_CONNECTION));
		}

		public async Task<HashSet<string>> GetBlackDomains()
		{
			var db = _redis.GetDatabase();
			var domains = await db.SetMembersAsync(RedisKeys.BLACK_DOMAINS);
			return domains.ToStringArray().ToHashSet();
		}

		public async Task SaveResolvedDomains(string key, IEnumerable<ResolvedDomain> domains)
		{
			if (!domains.Any())
				return;

			var protoSerialized = domains.Select(x => x.ProtoSerialize()).ToArray();
			var redisArray = protoSerialized.Select(x => (RedisValue)x).ToArray();
			var db = _redis.GetDatabase();
			var transaction = db.CreateTransaction();
			var deleteResult = transaction.KeyDeleteAsync(key);
			var addResult = transaction.ListRightPushAsync(key, redisArray);
			var transactionResult = await transaction.ExecuteAsync();
			_logger.LogInformation($"Redis Key - {key}, Transaction result - {transactionResult}, Added domains count - {await addResult}");
		}

		public async Task<IEnumerable<ResolvedDomain>> GetResolvedDomains(string key)
		{
			var db = _redis.GetDatabase();
			var redisArray = await db.ListRangeAsync(key);
			var domains = redisArray.Select(x => (byte[])x)
				.ToArray()
				.Select(x => x.ProtoDeserialize<ResolvedDomain>())
				.ToArray();
			return domains;
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
