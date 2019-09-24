using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Contracts.Protobuf;
using Dns.Resolver.Aggregator.Messages;
using Dns.Resolver.Aggregator.Models;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Aggregator.Services
{
	public class DomainAggregatorService : IDomainAggregatorService
	{
		private readonly DomainQueueCollection _domainCollection;
		private readonly ILogger<DomainAggregatorService> _logger;
		private readonly IDatabase _redisDb;
		private readonly IMessageQueue _messageQueue;

		private readonly string _redisBlackDomainsKey;
		private readonly string _redisWhiteDomainsKey;

		public DomainAggregatorService(ILogger<DomainAggregatorService> logger, ConnectionMultiplexer redis,
			IMessageQueue messageQueue, string redisBlackDomainKey, string redisWhiteDomainKey)
		{
			if (redis == null) throw new ArgumentNullException(nameof(redis));

			_logger = logger;
			_redisDb = redis.GetDatabase();
			_messageQueue = messageQueue;
			_redisBlackDomainsKey = redisBlackDomainKey;
			_redisWhiteDomainsKey = redisWhiteDomainKey;
			_domainCollection = new DomainQueueCollection();
			_domainCollection.NewIdAdded += DomainCollection_NewIdAdded;
		}

		private async void DomainCollection_NewIdAdded(object? sender, UniqueIdCountChangedArgs e)
		{
			_logger.LogInformation($"New Id {e.TraceId} Added, currenct count in queue: {e.Count}");
			await _messageQueue.PublishAsync(new DnsAnalyzerHealthCheckMessage("Dns.Resolver.Aggregator", "Завершен резолв доменов")).ConfigureAwait(false);
			if (e.Count > 2)
			{
				try
				{
					await StoreDomainsAsync().ConfigureAwait(false);
					NotifyCompletion(e.TraceId);
				}
				catch (Exception ex)
				{
					_logger.LogCritical(ex, ex.Message);
					throw;
				}
			}
		}

		public void AddDomain(DomainResolvedMessage domain)
		{
			if (domain == null) throw new ArgumentNullException(nameof(domain));

			_domainCollection.Add(domain);
		}

		public void NotifyCompletion(Guid traceId)
		{
			_messageQueue.Publish(new AnalyzeNeededMessage(traceId));
			_logger.LogInformation($"AnalyzeStartingEvent has published at {DateTimeOffset.Now}");
		}

		public async Task StoreDomainsAsync()
		{
			var domains = _domainCollection.DequeueDomains();

			var whiteDomains = domains.Where(x => x.DomainType == 2)
				.Select(x => new ResolvedDomain(x.Name, x.IPAddresses.ToHashSet()));
			var blackDomains = domains.Where(x => x.DomainType == 1)
				.Select(x => new ResolvedDomain(x.Name, x.IPAddresses.ToHashSet()));

			await StoreToRedisDomains(_redisWhiteDomainsKey, whiteDomains).ConfigureAwait(false);
			await StoreToRedisDomains(_redisBlackDomainsKey, blackDomains).ConfigureAwait(false);
		}

		private async Task StoreToRedisDomains(string key, IEnumerable<ResolvedDomain> domains)
		{
			var protoSerialized = domains.Select(x => x.ProtoSerialize()).ToArray();
			var redisArray = protoSerialized.Select(x => (RedisValue)x).ToArray();
			var transaction = _redisDb.CreateTransaction();
			var deleteResult = transaction.KeyDeleteAsync(key);
			var addTasks = new List<Task<long>>();
			foreach (var item in redisArray.SplitToMaxRedisChunks())
			{
				addTasks.Add(transaction.ListRightPushAsync(key, item.ToArray()));
			}
			var transactionResult = await transaction.ExecuteAsync().ConfigureAwait(false);
			var addCount = await Task.WhenAll(addTasks).ConfigureAwait(false);
		}
	}
}
