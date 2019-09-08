using System;
using System.Threading;
using System.Threading.Tasks;
using Dns.Analyzer.EventHandlers;
using Dns.Analyzer.Models;
using Dns.Analyzer.Services;
using Dns.Contracts.Events;
using Dns.DAL;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.RabbitMq;
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
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);
		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_BLACK_DOMAIN_RESOLVED = nameof(REDIS_BLACK_DOMAIN_RESOLVED);
		public const string REDIS_WHITE_DOMAIN_RESOLVED = nameof(REDIS_WHITE_DOMAIN_RESOLVED);
		public const string REDIS_VIGRUZKI_IPS = nameof(REDIS_VIGRUZKI_IPS);
		public const string REDIS_VIGRUZKI_SUBNETS = nameof(REDIS_VIGRUZKI_SUBNETS);
		public const string NOTIFY_SEND_CHANNEL = nameof(NOTIFY_SEND_CHANNEL);
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
		public const string ANALYZER_SUSPECT_IP_COUNT = nameof(ANALYZER_SUSPECT_IP_COUNT);

		public static int Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;

			EnvironmentExtensions.CheckVariables(
				PG_CONNECTION_STRING_READ,
				PG_CONNECTION_STRING_WRITE,
				REDIS_CONNECTION,
				REDIS_BLACK_DOMAIN_RESOLVED,
				REDIS_WHITE_DOMAIN_RESOLVED,
				REDIS_VIGRUZKI_IPS,
				REDIS_VIGRUZKI_SUBNETS,
				RABBITMQ_CONNECTION,
				NOTIFICATION_EMAIL_FROM,
				NOTIFY_SEND_CHANNEL,
				ANALYZER_SUSPECT_IP_COUNT
				);

			try
			{
				host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();
				ApplyMigrations(host.Services);
				var messageQueue = host.Services.GetRequiredService<IMessageQueue>();
				var handler = host.Services.GetRequiredService<AnalyzeStartingEventHandler>();
				messageQueue.Subscribe<AnalyzeStartingEvent, AnalyzeStartingEventHandler>(handler);

				host.Start();
				host.WaitForShutdown();
				host.Dispose();
				return 0;
			}
			catch (Exception ex)
			{
				if (_logger != null)
					_logger.LogCritical(ex, ex.Message);
				else Console.WriteLine(ex);
				host?.Dispose();
				return 1;
			}
		}

		private static void ApplyMigrations(IServiceProvider services)
		{
			var cntx = services.GetService<DnsDbContext>();
			cntx.Database.Migrate();
		}

		public static IHost CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureServices((_, services) =>
			{
				services.AddOptions();
				services.AddLogging(l => l.AddConsole());

				services.AddDbContext<DnsDbContext>(opt =>
					opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));
				services.AddDbContext<DnsReadOnlyDbContext>(opt =>
					opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_READ)));

				services.AddSingleton<ConnectionMultiplexer>(__ =>
				{
					var redisConnection = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
					return ConnectionMultiplexer.Connect(redisConnection);
				});

				services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
				{
					var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
					var factory = new ConnectionFactory() { HostName = EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION) };
					return new DefaultRabbitMQPersistentConnection(factory, logger);
				});

				services.AddSingleton<IMessageQueue, MessageQueueRabbitMQ>(sp =>
				{
					var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
					var logger = sp.GetRequiredService<ILogger<MessageQueueRabbitMQ>>();
					return new MessageQueueRabbitMQ(rabbitMQPersistentConnection, logger,
						queueName: string.Empty);
				});

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

				services.AddTransient<AnalyzeStartingEventHandler>(sp =>
				{
					var logger = sp.GetRequiredService<ILogger<AnalyzeStartingEventHandler>>();
					var analyze = sp.GetRequiredService<IAnalyzeService>();
					var notify = sp.GetRequiredService<INotifyService>();
					var ipInfo = sp.GetRequiredService<IIpInfoService>();
					var suspect = sp.GetRequiredService<SuspectDomainSevice>();
					var redis = sp.GetRequiredService<ConnectionMultiplexer>();
					var redisKeys = new RedisKeys(
						blackDomainsResolvedKey: EnvironmentExtensions.GetVariable(REDIS_BLACK_DOMAIN_RESOLVED),
						whiteDomainsResolvedKey: EnvironmentExtensions.GetVariable(REDIS_WHITE_DOMAIN_RESOLVED),
						vigruzkiIpKey: EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_IPS),
						vigruzkiSubnetKey: EnvironmentExtensions.GetVariable(REDIS_VIGRUZKI_SUBNETS),
						notifyMessageKey: EnvironmentExtensions.GetVariable(NOTIFY_SEND_CHANNEL));
					return new AnalyzeStartingEventHandler(logger, analyze, notify, ipInfo, suspect, redis, redisKeys);
				});
			})
			.Build();
	}
}