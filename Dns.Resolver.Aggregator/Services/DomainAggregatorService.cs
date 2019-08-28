using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dns.Contracts.Events;
using Dns.Contracts.Messages;
using Dns.Contracts.Protobuf;
using Dns.Resolver.Aggregator.Messages;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Aggregator.Services
{
	public class DomainAggregatorService : IDomainAggregatorService
	{
		// NEED FIX
		private readonly ConcurrentDictionary<Guid, List<DomainResolvedMessage>> _aggregateValues;
		private readonly ILogger<DomainAggregatorService> _logger;
		private readonly IDatabase _redisDb;
		private readonly IEventBus _eventBus;

		private readonly string _redisBlackDomainsKey;
		private readonly string _redisWhiteDomainsKey;

		public DomainAggregatorService(ILogger<DomainAggregatorService> logger, ConnectionMultiplexer redis,
			IEventBus eventBus, string redisBlackDomainKey, string redisWhiteDomainKey)
		{
			_aggregateValues = new ConcurrentDictionary<Guid, List<DomainResolvedMessage>>();
			_logger = logger;
			_redisDb = redis.GetDatabase();
			_eventBus = eventBus;
			_redisBlackDomainsKey = redisBlackDomainKey;
			_redisWhiteDomainsKey = redisWhiteDomainKey;
		}

		public async Task AddDomainAsync(DomainResolvedMessage domain)
		{
			_logger.LogInformation($"Adding domain {domain.Name} - {domain.TraceId}");
			_aggregateValues.AddOrUpdate(domain.TraceId,
				new List<DomainResolvedMessage>() { domain },
				(_, val) => val.Append(domain).ToList());

			if (_aggregateValues.Keys.Count > 2)
			{
				await StoreDomainsAsync().ConfigureAwait(false);
				NotifyCompletion();
			}
			else
			{
				await Task.CompletedTask;
			}
		}

		public void NotifyCompletion()
		{
			_eventBus.Publish(new AnalyzeStartingEvent());
		}

		public async Task StoreDomainsAsync()
		{
			var oldest = _aggregateValues.Keys.First();
			if(_aggregateValues.TryRemove(oldest, out List<DomainResolvedMessage>? domains))
			{
				var whiteDomains = domains.Where(x => x.DomainType == 2)
					.Select(x => new ResolvedDomain(x.Name, x.IPAddresses.ToHashSet()));
				var blackDomains = domains.Where(x => x.DomainType == 1)
					.Select(x => new ResolvedDomain(x.Name, x.IPAddresses.ToHashSet()));

				await StoreToRedisDomains(_redisWhiteDomainsKey, whiteDomains).ConfigureAwait(false);
				await StoreToRedisDomains(_redisBlackDomainsKey, blackDomains).ConfigureAwait(false);
			}
		}

		private async Task StoreToRedisDomains(string key, IEnumerable<ResolvedDomain> domains)
		{
			var protoSerialized = domains.Select(x => x.ProtoSerialize()).ToArray();
			var redisArray = protoSerialized.Select(x => (RedisValue)x).ToArray();
			var transaction = _redisDb.CreateTransaction();
			var deleteResult = transaction.KeyDeleteAsync(key);
			var addResult = transaction.ListRightPushAsync(key, redisArray);
			var transactionResult = await transaction.ExecuteAsync().ConfigureAwait(false);
			var addCount = await addResult.ConfigureAwait(false);
		}
	}
}
