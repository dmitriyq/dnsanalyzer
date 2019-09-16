using System;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Resolver.Consumer.Messages;
using Dns.Resolver.Consumer.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
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

		public static void Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			try
			{
				EnvironmentExtensions.CheckVariables(
					HYPERLOCAL_SERVER,
					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_DOMAINS_QUEUE
					);

				var host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();

				var _messageQueue = host.Services.GetRequiredService<IMessageQueue>();
				var handler = host.Services.GetRequiredService<DomainPublishMessageHandler>();
				var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_DNS_DOMAINS_QUEUE);
				_messageQueue.Subscribe<DomainPublishMessage, DomainPublishMessageHandler>(queueName, handler);

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

				services.AddMessageBus(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));
				services.AddSingleton<IMessageQueue, MessageQueueEasyNetQ>();

				services.AddTransient<IDomainLookupService, DomainLookupService>(sp => {
					var dnsServer = EnvironmentExtensions.GetVariable(HYPERLOCAL_SERVER);
					var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
					return new DomainLookupService(logger, dnsServer);
				});
				services.AddTransient<DomainPublishMessageHandler>();
			})
			.Build();
	}
}
