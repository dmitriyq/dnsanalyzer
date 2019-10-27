using System;
using System.Threading;
using Dns.Contracts.Messages;
using Dns.Contracts.Services;
using Dns.DAL;
using Dns.Resolver.Analyzer.Messages;
using Dns.Resolver.Analyzer.Services.Implementation;
using Dns.Resolver.Analyzer.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Analyzer
{
	public sealed class Program
	{
		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public const string RABBITMQ_ANALYZE_QUEUE = nameof(RABBITMQ_ANALYZE_QUEUE);
		public const string RABBITMQ_ANALYZE_BATCH_QUEUE = nameof(RABBITMQ_ANALYZE_BATCH_QUEUE);
		public const string RABBITMQ_SUSPECT_QUEUE = nameof(RABBITMQ_SUSPECT_QUEUE);
		public const string RABBITMQ_SUSPECT_BATCH_QUEUE = nameof(RABBITMQ_SUSPECT_BATCH_QUEUE);

		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);

		public const string REDIS_VIGRUZKI_IPS = nameof(REDIS_VIGRUZKI_IPS);
		public const string REDIS_VIGRUZKI_SUBNETS = nameof(REDIS_VIGRUZKI_SUBNETS);

		public const string ANALYZE_EXPIRE_INTERVAL = nameof(ANALYZE_EXPIRE_INTERVAL);
		public const string ANALYZE_CLOSE_INTERVAL = nameof(ANALYZE_CLOSE_INTERVAL);
		public const string ANALYZE_FALSE_POSITIVE_INTERVAL = nameof(ANALYZE_FALSE_POSITIVE_INTERVAL);

		public static void Main(string[] args)
		{
			ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount * 10);
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new string[]
				{
					REDIS_CONNECTION,
					RABBITMQ_CONNECTION,
					RABBITMQ_ANALYZE_QUEUE,
					RABBITMQ_ANALYZE_BATCH_QUEUE,
					RABBITMQ_SUSPECT_QUEUE,
					RABBITMQ_SUSPECT_BATCH_QUEUE,
					PG_CONNECTION_STRING_WRITE,
					PG_CONNECTION_STRING_READ,
					NOTIFICATION_EMAIL_FROM,
					REDIS_VIGRUZKI_IPS,
					REDIS_VIGRUZKI_SUBNETS,
					ANALYZE_CLOSE_INTERVAL,
					ANALYZE_EXPIRE_INTERVAL,
					ANALYZE_FALSE_POSITIVE_INTERVAL
				},
				configureServices: (_, services) =>
				{
					services.AddOptions();
					services.AddLogging(l => l.AddConsole()
						.SetEFCoreLogLevel(LogLevel.Warning));

					services.AddSingleton<ConnectionMultiplexer>(__ =>
						ConnectionMultiplexer.Connect(EnvironmentExtensions.GetVariable(REDIS_CONNECTION)));

					services.AddMemoryCache();
					services.AddStackExchangeRedisCache(opts =>
					{
						opts.Configuration = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
						opts.InstanceName = typeof(Dns.Resolver.Analyzer.Program).Namespace;
					});

					services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

					services.AddDbContextPool<DnsDbContext>(optionsAction: opt =>
						opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));

					services.AddTransient<INotifyService, NotifyService>(sp =>
					{
						var emailFrom = EnvironmentExtensions.GetVariable(NOTIFICATION_EMAIL_FROM);
						return new NotifyService(sp, emailFrom);
					});
					services.AddTransient<ICacheService, CacheService>(sp =>
					{
						var distCache = sp.GetRequiredService<IDistributedCache>();
						var memCache = sp.GetRequiredService<IMemoryCache>();
						var redis = sp.GetRequiredService<ConnectionMultiplexer>();

						var keys = (EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_IPS),
									EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_SUBNETS));
						return new CacheService(distCache, memCache, redis, keys);
					});
					services.AddTransient<IAnalyzeService, AnalyzeService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<AnalyzeService>>();
						var cache = sp.GetRequiredService<ICacheService>();

						var intervals = (expire: TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(ANALYZE_EXPIRE_INTERVAL))),
							closing: TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(ANALYZE_CLOSE_INTERVAL))),
							falsePositive: TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(ANALYZE_FALSE_POSITIVE_INTERVAL))));
						return new AnalyzeService(sp, logger, cache,
							expireInterval: intervals.expire,
							closingInterval: intervals.closing,
							falsePositiveInterval: intervals.falsePositive);
					});
					services.AddTransient<IAnalyzeUpdateService, AnalyzeUpdateService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<AnalyzeUpdateService>>();
						var refreshInterval = TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(ANALYZE_EXPIRE_INTERVAL)));
						var analyze = sp.GetRequiredService<IAnalyzeService>();
						var notify = sp.GetRequiredService<INotifyService>();
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						return new AnalyzeUpdateService(logger, refreshInterval, analyze, notify, messageBus);
					});
					services.AddSingleton<BatchingAttackService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<BatchingAttackService>>();
						var refreshInterval = TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(ANALYZE_EXPIRE_INTERVAL))).Divide(3d);
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						return new BatchingAttackService(logger, refreshInterval, messageBus);
					});
					services.AddSingleton<BatchingSuspectService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<BatchingSuspectService>>();
						var refreshInterval = TimeSpan.FromMinutes(10);
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						return new BatchingSuspectService(logger, refreshInterval, messageBus);
					});
					services.AddTransient<AttackBatchCreatedMessageHandler>(sp =>
					{
						var analyze = sp.GetRequiredService<IAnalyzeService>();
						var notify = sp.GetRequiredService<INotifyService>();
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						return new AttackBatchCreatedMessageHandler(analyze, notify, messageBus);
					});
					services.AddTransient<AttackFoundMessageHandler>(sp =>
					{
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						var batchService = sp.GetRequiredService<BatchingAttackService>();
						return new AttackFoundMessageHandler(messageBus, batchService);
					});
					services.AddTransient<SuspectBatchCreatedMessageHandler>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<SuspectBatchCreatedMessageHandler>>();
						var db = sp.GetRequiredService<DnsDbContext>();
						return new SuspectBatchCreatedMessageHandler(db, logger);
					});
					services.AddTransient<SuspectDomainFoundMessageHandler>(sp =>
					{
						var batchService = sp.GetRequiredService<BatchingSuspectService>();
						return new SuspectDomainFoundMessageHandler(batchService);
					});
				},
				beforeHostStartAction: services =>
				{
					services.ApplyDbMigration<DnsDbContext>();

					var messageBus = services.GetRequiredService<IMessageQueue>();
					var attackFoundHandler = services.GetRequiredService<AttackFoundMessageHandler>();
					var attackFroundSubId = EnvironmentExtensions.GetVariable(RABBITMQ_ANALYZE_QUEUE);
					messageBus.Subscribe<AttackFoundMessage, AttackFoundMessageHandler>(attackFroundSubId, attackFoundHandler);

					var attackBatchHandler = services.GetRequiredService<AttackBatchCreatedMessageHandler>();
					var attackBatchSubId = EnvironmentExtensions.GetVariable(RABBITMQ_ANALYZE_BATCH_QUEUE);
					messageBus.Subscribe<AttackBatchCreatedMessage, AttackBatchCreatedMessageHandler>(attackBatchSubId, attackBatchHandler);

					var suspectFoundHandler = services.GetRequiredService<SuspectDomainFoundMessageHandler>();
					var suspectFoundSubId = EnvironmentExtensions.GetVariable(RABBITMQ_SUSPECT_QUEUE);
					messageBus.Subscribe<SuspectDomainFoundMessage, SuspectDomainFoundMessageHandler>(attackFroundSubId, suspectFoundHandler);

					var suspectBatchHandler = services.GetRequiredService<SuspectBatchCreatedMessageHandler>();
					var suspectBatchSubId = EnvironmentExtensions.GetVariable(RABBITMQ_SUSPECT_BATCH_QUEUE);
					messageBus.Subscribe<SuspectBatchCreatedMessage, SuspectBatchCreatedMessageHandler>(suspectBatchSubId, suspectBatchHandler);

					var updateSvc = services.GetRequiredService<IAnalyzeUpdateService>();
					_ = updateSvc.RunJobAsync();
				});
		}
	}
}
