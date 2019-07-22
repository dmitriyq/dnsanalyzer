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

			_logger.LogInformation($"Begin serialization of {domains.Count()} domain");
			var protoSerialized = domains.Select(x => x.ProtoSerialize()).ToArray();
			_logger.LogInformation($"Begin cast to RedisValue byte arrays");
			var redisArray = protoSerialized.Select(x => (RedisValue)x).ToArray();
			_logger.LogInformation($"Begin transaction");
			var db = _redis.GetDatabase();
			var transaction = db.CreateTransaction();
			var deleteResult = transaction.KeyDeleteAsync(key);
			var addResult = transaction.ListRightPushAsync(key, redisArray);
			var transactionResult = await transaction.ExecuteAsync();
			_logger.LogInformation($"Redis Transaction result - {transactionResult}, Added domains count - {await addResult}");
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
