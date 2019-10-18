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

		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
		public const string NOTIFY_SEND_CHANNEL = nameof(NOTIFY_SEND_CHANNEL);

		public const string REDIS_VIGRUZKI_IPS = nameof(REDIS_VIGRUZKI_IPS);
		public const string REDIS_VIGRUZKI_SUBNETS = nameof(REDIS_VIGRUZKI_SUBNETS);

		public const string ANALYZE_EXPIRE_INTERVAL = nameof(ANALYZE_EXPIRE_INTERVAL);
		public const string ANALYZE_CLOSE_INTERVAL = nameof(ANALYZE_CLOSE_INTERVAL);
		public const string ANALYZE_FALSE_POSITIVE_INTERVAL = nameof(ANALYZE_FALSE_POSITIVE_INTERVAL);

		public static void Main(string[] args)
		{
			ThreadPool.SetMinThreads(50, 150);
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new string[]
				{
					REDIS_CONNECTION,
					RABBITMQ_CONNECTION,
					RABBITMQ_ANALYZE_QUEUE,
					PG_CONNECTION_STRING_WRITE,
					PG_CONNECTION_STRING_READ,
					NOTIFICATION_EMAIL_FROM,
					NOTIFY_SEND_CHANNEL,
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
					//services.AddDbContextPool<DnsReadOnlyDbContext>(optionsAction: opt =>
					//	opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_READ)));

					services.AddTransient<INotifyService, NotifyService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<NotifyService>>();
						var db = sp.GetRequiredService<DnsDbContext>();
						var emailFrom = EnvironmentExtensions.GetVariable(NOTIFICATION_EMAIL_FROM);
						return new NotifyService(logger, db, emailFrom);
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
						var redis = sp.GetRequiredService<ConnectionMultiplexer>();
						var notifyChannel = EnvironmentExtensions.GetVariable(NOTIFY_SEND_CHANNEL);
						return new AnalyzeUpdateService(logger, refreshInterval, analyze, notify, redis, notifyChannel);
					});
					services.AddTransient<AttackFoundMessageHandler>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<AttackFoundMessageHandler>>();
						var analyze = sp.GetRequiredService<IAnalyzeService>();
						var notify = sp.GetRequiredService<INotifyService>();
						var redis = sp.GetRequiredService<ConnectionMultiplexer>();
						var messageBus = sp.GetRequiredService<IMessageQueue>();
						var notifyChannel = EnvironmentExtensions.GetVariable(NOTIFY_SEND_CHANNEL);
						return new AttackFoundMessageHandler(logger, analyze, notify, redis, messageBus, notifyChannel);
					});
				},
				beforeHostStartAction: services =>
				{
					services.ApplyDbMigration<DnsDbContext>();

					var messageBus = services.GetRequiredService<IMessageQueue>();
					var handler = services.GetRequiredService<AttackFoundMessageHandler>();
					var subId = EnvironmentExtensions.GetVariable(RABBITMQ_ANALYZE_QUEUE);
					messageBus.Subscribe<AttackFoundMessage, AttackFoundMessageHandler>(subId, handler);

					var updateSvc = services.GetRequiredService<IAnalyzeUpdateService>();
					_ = updateSvc.RunJobAsync();
				});
		}
	}
}
