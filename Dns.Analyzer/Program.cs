using System;
using System.Threading;
using System.Threading.Tasks;
using Dns.Analyzer.EventHandlers;
using Dns.Analyzer.Models;
using Dns.Analyzer.Services;
using Dns.Contracts.Messages;
using Dns.DAL;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.RabbitMq;
using Grfc.Library.EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Dns.Analyzer
{
	public sealed class Program
	{
		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_BLACK_DOMAIN_RESOLVED = nameof(REDIS_BLACK_DOMAIN_RESOLVED);
		public const string REDIS_WHITE_DOMAIN_RESOLVED = nameof(REDIS_WHITE_DOMAIN_RESOLVED);
		public const string REDIS_VIGRUZKI_IPS = nameof(REDIS_VIGRUZKI_IPS);
		public const string REDIS_VIGRUZKI_SUBNETS = nameof(REDIS_VIGRUZKI_SUBNETS);
		public const string NOTIFY_SEND_CHANNEL = nameof(NOTIFY_SEND_CHANNEL);
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_ANALYZE_QUEUE = nameof(RABBITMQ_ANALYZE_QUEUE);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
		public const string ANALYZER_SUSPECT_IP_COUNT = nameof(ANALYZER_SUSPECT_IP_COUNT);

		public static void Main(string[] args)
		{
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new[]
				{
					PG_CONNECTION_STRING_WRITE,
					REDIS_CONNECTION,
					REDIS_BLACK_DOMAIN_RESOLVED,
					REDIS_WHITE_DOMAIN_RESOLVED,
					REDIS_VIGRUZKI_IPS,
					REDIS_VIGRUZKI_SUBNETS,
					RABBITMQ_CONNECTION,
					RABBITMQ_ANALYZE_QUEUE,
					NOTIFY_SEND_CHANNEL,
					NOTIFICATION_EMAIL_FROM,
					ANALYZER_SUSPECT_IP_COUNT
				},
				configureServices:
					(_, services) =>
					{
						services.AddOptions();
						services.AddLogging(l => l.AddConsole()
							.SetEFCoreLogLevel(LogLevel.Warning));
						services.AddDbContext<DnsDbContext>(opt =>
							opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));

						services.AddSingleton<ConnectionMultiplexer>(__ =>
						{
							var redisConnection = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
							return ConnectionMultiplexer.Connect(redisConnection);
						});

						services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

						services.AddTransient<SuspectDomainSevice>(sp =>
						{
							var logger = sp.GetRequiredService<ILogger<SuspectDomainSevice>>();
							var dbContext = sp.GetRequiredService<DnsDbContext>();
							var suspectIpCount = int.Parse(EnvironmentExtensions.GetVariable(ANALYZER_SUSPECT_IP_COUNT));
							return new SuspectDomainSevice(logger, dbContext, suspectIpCount);
						});
						services.AddTransient<IAnalyzeService, AnalyzeService>();
						services.AddTransient<IIpInfoService, IpInfoService>();
						services.AddTransient<INotifyService, NotifyService>(sp =>
						{
							var logger = sp.GetRequiredService<ILogger<NotifyService>>();
							var dbContext = sp.GetRequiredService<DnsDbContext>();
							var emailFrom = EnvironmentExtensions.GetVariable(NOTIFICATION_EMAIL_FROM);
							return new NotifyService(logger, dbContext, emailFrom);
						});

						services.AddTransient<AnalyzeNeededMessageHandler>(sp =>
						{
							var logger = sp.GetRequiredService<ILogger<AnalyzeNeededMessageHandler>>();
							var analyze = sp.GetRequiredService<IAnalyzeService>();
							var notify = sp.GetRequiredService<INotifyService>();
							var ipInfo = sp.GetRequiredService<IIpInfoService>();
							var suspect = sp.GetRequiredService<SuspectDomainSevice>();
							var redis = sp.GetRequiredService<ConnectionMultiplexer>();
							var messageQueue = sp.GetRequiredService<IMessageQueue>();
							var redisKeys = new RedisKeys(
								blackDomainsResolvedKey: EnvironmentExtensions.GetVariable(REDIS_BLACK_DOMAIN_RESOLVED),
								whiteDomainsResolvedKey: EnvironmentExtensions.GetVariable(REDIS_WHITE_DOMAIN_RESOLVED),
								vigruzkiIpKey: EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_IPS),
								vigruzkiSubnetKey: EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_SUBNETS),
								notifyMessageKey: EnvironmentExtensions.GetVariable(NOTIFY_SEND_CHANNEL));
							return new AnalyzeNeededMessageHandler(logger, analyze, notify, ipInfo, suspect, redis, redisKeys, messageQueue);
						});
					},
				beforeHostStartAction:
					services =>
					{
						services.ApplyDbMigration<DnsDbContext>();
						var messageQueue = services.GetRequiredService<IMessageQueue>();
						var handler = services.GetRequiredService<AnalyzeNeededMessageHandler>();
						var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_ANALYZE_QUEUE);
						messageQueue.Subscribe<AnalyzeNeededMessage, AnalyzeNeededMessageHandler>(queueName, handler);
					}
				);
		}
	}
}