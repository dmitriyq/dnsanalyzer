using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Dns.DAL;
using Dns.Resolver.Producer.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
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

		public static void Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			try
			{
				EnvironmentExtensions.CheckVariables(
					RESOLVER_PUBLISHER_DELAY_SEC,

					PG_CONNECTION_STRING_READ,
					PG_CONNECTION_STRING_WRITE,

					REDIS_CONNECTION,
					REDIS_BLACK_DOMAINS,

					RABBITMQ_CONNECTION
					);

				var host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();
				var worker = host.Services.GetRequiredService<PublishWorker>();
				ApplyMigrations(host.Services);
				_ = worker.RunJob();
				host.Start();
				host.WaitForShutdown();
				host.Dispose();
			}
			catch (Exception ex)
			{
				if (_logger != null)
					_logger.LogCritical(ex, ex.Message);
				else Console.WriteLine(ex);
				throw;
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

				services.AddMessageBus(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

				services.AddSingleton<IMessageQueue, MessageQueueEasyNetQ>();

				services.AddTransient<IDomainService, DomainService>();
				services.AddTransient<PublishWorker>(sp =>
				{
					var logger = sp.GetRequiredService<ILogger<PublishWorker>>();
					var messageQueue = sp.GetRequiredService<IMessageQueue>();
					var domainSvc = sp.GetRequiredService<IDomainService>();

					return new PublishWorker(logger, messageQueue, domainSvc);
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
