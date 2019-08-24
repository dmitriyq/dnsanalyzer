using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dns.Library;
using Dns.Resolver.Subscriber.Events;
using Dns.Resolver.Subscriber.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Dns.Resolver.Subscriber
{
	public class Program
	{
		public static int Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;
			try
			{
				EnvironmentExtensions.CheckVariables(
					EnvVars.HYPERLOCAL_SERVER,
					EnvVars.RABBITMQ_CONNECTION,
					EnvVars.RABBITMQ_DNS_DOMAINS_QUEUE
					);

				host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();

				var _eventBus = host.Services.GetRequiredService<IEventBus>();
				_eventBus.Subscribe<DomainPublisherEvent, DomainPublisherEventHandler>();

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
			.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureServices((_, services) =>
			{
				services.AddOptions();
				services.AddLogging(l => l.AddConsole());

				services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
				{
					var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
					var factory = new ConnectionFactory() { HostName = EnvironmentExtensions.GetVariable(EnvVars.RABBITMQ_CONNECTION) };
					return new DefaultRabbitMQPersistentConnection(factory, logger);
				});

				services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
				{
					var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
					var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
					var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
					var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
					return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager,
						queueName: EnvironmentExtensions.GetVariable(EnvVars.RABBITMQ_DNS_DOMAINS_QUEUE));
				});
				services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
				services.AddTransient<IDomainLookupService, DomainLookupService>(sp => {
					var dnsServer = EnvironmentExtensions.GetVariable(EnvVars.HYPERLOCAL_SERVER);
					var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
					return new DomainLookupService(logger, dnsServer);
				});
				services.AddTransient<DomainPublisherEventHandler>();

				var container = new ContainerBuilder();
				container.Populate(services);
			})
			.Build();
	}
}
