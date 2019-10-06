using System;
using Dns.Contracts.Messages;
using Dns.Resolver.Blacklist.Messages;
using Dns.Resolver.Blacklist.Services.Implementation;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Blacklist
{
	public sealed class Program
	{
		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_WHITE_LIST_CACHE = nameof(REDIS_WHITE_LIST_CACHE);
		public const string REDIS_BLACK_DOMAINS = nameof(REDIS_BLACK_DOMAINS);
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public const string RABBITMQ_BLACK_DOMAIN_RESOLVE_QUEUE = nameof(RABBITMQ_BLACK_DOMAIN_RESOLVE_QUEUE);
		public const string RABBITMQ_WHITE_DOMAIN_LIST_UPDATE_QUEUE = nameof(RABBITMQ_WHITE_DOMAIN_LIST_UPDATE_QUEUE);

		public const string HYPERLOCAL_SERVER = nameof(HYPERLOCAL_SERVER);
		public const string MAX_CONCURRENT_RESOLVE = nameof(MAX_CONCURRENT_RESOLVE);

		public const string REFRESH_INTERVAL = nameof(REFRESH_INTERVAL);

		public static void Main(string[] args)
		{
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new string[]
				{
					REDIS_CONNECTION,
					REDIS_WHITE_LIST_CACHE,
					REDIS_BLACK_DOMAINS,
					RABBITMQ_CONNECTION,
					RABBITMQ_BLACK_DOMAIN_RESOLVE_QUEUE,
					RABBITMQ_WHITE_DOMAIN_LIST_UPDATE_QUEUE,
					HYPERLOCAL_SERVER,
					MAX_CONCURRENT_RESOLVE,
					REFRESH_INTERVAL
				},
				configureServices: (_, services) =>
				{
					services.AddOptions();
					services.AddLogging(l => l.AddConsole()
						.SetEFCoreLogLevel(LogLevel.Warning));

					services.AddSingleton<ConnectionMultiplexer>(__ =>
						ConnectionMultiplexer.Connect(EnvironmentExtensions.GetVariable(REDIS_CONNECTION)));

					services.AddStackExchangeRedisCache(opts =>
					{
						opts.Configuration = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
						opts.InstanceName = typeof(Dns.Resolver.Blacklist.Program).Namespace;
					});

					services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

					services.AddTransient<ICacheService, CacheService>(sp =>
					{
						var disCache = sp.GetRequiredService<IDistributedCache>();
						var memCache = sp.GetRequiredService<IMemoryCache>();
						var logger = sp.GetRequiredService<ILogger<CacheService>>();
						var redis = sp.GetRequiredService<ConnectionMultiplexer>();
						var whitelistKey = EnvironmentExtensions.GetVariable(REDIS_WHITE_LIST_CACHE);
						var blackListKey = EnvironmentExtensions.GetVariable(REDIS_BLACK_DOMAINS);
						return new CacheService(disCache, memCache, redis, logger, whitelistKey, blackListKey);
					});
					services.AddTransient<IDomainLookupService, DomainLookupService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
						var dnsServer = EnvironmentExtensions.GetVariable(HYPERLOCAL_SERVER);
						var concurrentRequests = int.Parse(EnvironmentExtensions.GetVariable(MAX_CONCURRENT_RESOLVE));
						return new DomainLookupService(logger, dnsServer, concurrentRequests);
					});
					services.AddTransient<IBlacklistUpdateService, BlacklistUpdateService>(sp =>
					{
						var disCache = sp.GetRequiredService<IDistributedCache>();
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						var cacheSvc = sp.GetRequiredService<ICacheService>();
						var logger = sp.GetRequiredService<ILogger<BlacklistUpdateService>>();
						var updateInterval = TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(REFRESH_INTERVAL)));
						return new BlacklistUpdateService(disCache, messageBus, cacheSvc, logger, updateInterval);
					});
					services.AddTransient<WhiteListUpdatedMessageHandler>();
					services.AddTransient<BlackDomainResolveNeededMessageHandler>();
				},
				beforeHostStartAction: services =>
				{
					var messageBus = services.GetRequiredService<IMessageQueue>();
					var whiteListHandler = services.GetRequiredService<WhiteListUpdatedMessageHandler>();
					var blackDomainHandler = services.GetRequiredService<BlackDomainResolveNeededMessageHandler>();

					var subWhiteId = EnvironmentExtensions.GetVariable(RABBITMQ_WHITE_DOMAIN_LIST_UPDATE_QUEUE);
					messageBus.Subscribe<WhiteListUpdatedMessage, WhiteListUpdatedMessageHandler>(subWhiteId, whiteListHandler);

					var blackDomainId = EnvironmentExtensions.GetVariable(RABBITMQ_BLACK_DOMAIN_RESOLVE_QUEUE);
					messageBus.Subscribe<BlackDomainResolveNeededMessage, BlackDomainResolveNeededMessageHandler>(blackDomainId, blackDomainHandler);

					var updateService = services.GetRequiredService<IBlacklistUpdateService>();
					_ = updateService.RunJobAsync();
				});
		}
	}
}
