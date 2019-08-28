using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dns.Contracts.Messages;
using Dns.Resolver.Consumer.Messages;
using Dns.Resolver.Consumer.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Dns.Resolver.Consumer
{
	public class Program
	{
		public const string HYPERLOCAL_SERVER = nameof(HYPERLOCAL_SERVER);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_DNS_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_DOMAINS_QUEUE);
		public const string RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE);

		public static int Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;
			try
			{
				EnvironmentExtensions.CheckVariables(
					HYPERLOCAL_SERVER,
					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_DOMAINS_QUEUE,
					RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE
					);

				host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();

				var _messageQueue = host.Services.GetRequiredService<IMessageQueue>();
				var handler = host.Services.GetRequiredService<DomainPublishMessageHandler>();
				_messageQueue.HandleMessage<DomainPublishMessage, DomainPublishMessageHandler>(handler);

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
					var factory = new ConnectionFactory() { HostName = EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION) };
					return new DefaultRabbitMQPersistentConnection(factory, logger);
				});

				services.AddSingleton<IMessageQueue, MessageQueueRabbitMQ>(sp =>
				{
					var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
					var logger = sp.GetRequiredService<ILogger<MessageQueueRabbitMQ>>();
					return new MessageQueueRabbitMQ(rabbitMQPersistentConnection, logger,
						queueName: EnvironmentExtensions.GetVariable(RABBITMQ_DNS_DOMAINS_QUEUE));
				});
				services.AddTransient<IDomainLookupService, DomainLookupService>(sp => {
					var dnsServer = EnvironmentExtensions.GetVariable(HYPERLOCAL_SERVER);
					var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
					return new DomainLookupService(logger, dnsServer);
				});
				services.AddTransient<DomainPublishMessageHandler>(sp => {
					var domainLookup = sp.GetRequiredService<IDomainLookupService>();
					var messageQueue = sp.GetRequiredService<IMessageQueue>();
					var resolvedQueue = EnvironmentExtensions.GetVariable(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE);
					return new DomainPublishMessageHandler(domainLookup, messageQueue, resolvedQueue);
				});

				var container = new ContainerBuilder();
				container.Populate(services);
			})
			.Build();
	}
}
