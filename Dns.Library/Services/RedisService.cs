using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Library.Models;
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

		public async Task<HashSet<string>> GetStringSetMembers(string key)
		{
			var db = _redis.GetDatabase();
			var set = await db.SetMembersAsync(key);
			return set.ToStringArray().ToHashSet();
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

		public async Task Publish(string channel, RedisValue message)
		{
			var subscriber = _redis.GetSubscriber();
			await subscriber.PublishAsync(channel, message);
		}
		public async Task Subscribe(string channel, Action<string> onMessage)
		{
			var subscriber = _redis.GetSubscriber();
			await subscriber.SubscribeAsync(channel, (channel, message) => onMessage(message));
		}
	}
	public class RedisKeys
	{
		public const string BLACK_DOMAINS = "Vigruzki_Domains";
		public const string BLACK_IPS = "Vigruzki_Ips";
		public const string BLACK_SUBNETS = "Vigruzki_Subnets";

		public const string BLACK_DOMAINS_RESOLVED = "DNS_Resolver_Black";
		public const string WHITE_DOMAINS_RESOLVED = "DNS_Resolver_White";

		public const string RESOLVE_COMPLETE_CHANNEL = "DNS_Resolve_Complete";

		public const string NOTIFY_SEND_CHANNEL = "Notification_Send_Message";
	}
}
