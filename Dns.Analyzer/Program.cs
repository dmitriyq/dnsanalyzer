using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dns.Analyzer.Services;
using Dns.DAL;
using Dns.Library;
using Dns.Library.Services;
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

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
		public const string ANALYZER_SUSPECT_IP_COUNT = nameof(ANALYZER_SUSPECT_IP_COUNT);

		public static ManualResetEventSlim resetEventSlim = new ManualResetEventSlim();

		public static void Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;

			EnvironmentExtensions.CheckVariables(
				PG_CONNECTION_STRING_READ,
				PG_CONNECTION_STRING_WRITE,
				REDIS_CONNECTION,
				REDIS_BLACK_DOMAIN_RESOLVED,
				REDIS_WHITE_DOMAIN_RESOLVED,
				NOTIFICATION_EMAIL_FROM,
				ANALYZER_SUSPECT_IP_COUNT
				);

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			var services = serviceCollection.BuildServiceProvider();
			ApplyMigrations(services);

			var bootstrapper = services.GetService<Bootstrapper>();
			Task.Run(async () =>
			{
				await bootstrapper.StartAnalyzer();
			});

			resetEventSlim.Wait();
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions();
			services.AddLogging(l => l.AddConsole());
			services.AddDbContext<DnsDbContext>(opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(EnvVars.PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));
			services.AddDbContext<DnsReadOnlyDbContext>(opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(EnvVars.PG_CONNECTION_STRING_READ)));

			services.AddTransient<AttackService>();
			services.AddTransient<SuspectDomainSevice>();
			services.AddTransient<IpInfoService>();
			services.AddTransient<Bootstrapper>();
			services.AddTransient<NotifyService>();

			services.AddSingleton<RedisService>();
		}

		private static void ApplyMigrations(IServiceProvider services)
		{
			var cntx = services.GetService<DnsDbContext>();
			cntx.Database.Migrate();
		}

		public static IHost CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureServices((_, services) =>
			{
				services.AddOptions();
				services.AddLogging(l => l.AddConsole());

				services.AddSingleton<ConnectionMultiplexer>(sp =>
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

				services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
				{
					var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
					var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
					var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
					var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
					return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager);
				});
				services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

				services.AddTransient<SuspectDomainSevice>();
			})
			.Build();
	}
}