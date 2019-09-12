using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Dns.DAL;
using Dns.Resolver.Producer.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Dns.Resolver.Producer
{
	public class Program
	{
		public const string RESOLVER_PUBLISHER_DELAY_SEC = nameof(RESOLVER_PUBLISHER_DELAY_SEC);

		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_BLACK_DOMAINS = nameof(REDIS_BLACK_DOMAINS);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_DNS_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_DOMAINS_QUEUE);
		public const string RABBITMQ_HEALTH_QUEUE = nameof(RABBITMQ_HEALTH_QUEUE);

		public const string RABBITMQ_PRODUCER_LIMIT = nameof(RABBITMQ_PRODUCER_LIMIT);
		public const string RABBITMQ_PRODUCER_LIMIT_TIMEOUT = nameof(RABBITMQ_PRODUCER_LIMIT_TIMEOUT);

		public static int Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;
			try
			{
				EnvironmentExtensions.CheckVariables(
					RESOLVER_PUBLISHER_DELAY_SEC,

					PG_CONNECTION_STRING_READ,
					PG_CONNECTION_STRING_WRITE,

					REDIS_CONNECTION,
					REDIS_BLACK_DOMAINS,

					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_DOMAINS_QUEUE,

					RABBITMQ_PRODUCER_LIMIT,
					RABBITMQ_PRODUCER_LIMIT_TIMEOUT
					);

				host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();
				ApplyMigrations(host.Services);
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
					ConnectionMultiplexer.Connect(EnvironmentExtensions.GetVariable(REDIS_CONNECTION)));

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

				services.AddTransient<IDomainService, DomainService>();
				services.AddHostedService<PublishWorker>(sp => 
				{
					var logger = sp.GetRequiredService<ILogger<PublishWorker>>();
					var messageQueue = sp.GetRequiredService<IMessageQueue>();
					var domainSvc = sp.GetRequiredService<IDomainService>();
					var ihostLifeTime = sp.GetRequiredService<IHostApplicationLifetime>();
					var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_DNS_DOMAINS_QUEUE);
					var healthQueue = EnvironmentExtensions.GetVariable(RABBITMQ_HEALTH_QUEUE);

					var limit = int.Parse(EnvironmentExtensions.GetVariable(RABBITMQ_PRODUCER_LIMIT));
					var timeout = int.Parse(EnvironmentExtensions.GetVariable(RABBITMQ_PRODUCER_LIMIT_TIMEOUT));

					return new PublishWorker(logger, messageQueue, domainSvc, ihostLifeTime, queueName, healthQueue, limit, timeout);
				});

			})
			.Build();

		private static void ApplyMigrations(IServiceProvider services)
		{
			var cntx = services.GetService<DnsDbContext>();
			cntx.Database.Migrate();
		}
	}
}
