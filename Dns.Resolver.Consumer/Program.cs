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

		public const string MAX_CONCURRENT_RESOLVE = nameof(MAX_CONCURRENT_RESOLVE);

		public static void Main(string[] args)
		{
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new[]
				{
					HYPERLOCAL_SERVER,
					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_DOMAINS_QUEUE,
					MAX_CONCURRENT_RESOLVE
				},
				configureServices: (_, services) =>
				{
					services.AddOptions();
					services.AddLogging(l => l.AddConsole()
						.SetEFCoreLogLevel(LogLevel.Warning));

					services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

					services.AddTransient<IDomainLookupService, DomainLookupService>(sp =>
					{
						var dnsServer = EnvironmentExtensions.GetVariable(HYPERLOCAL_SERVER);
						var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
						var parallelCount = int.Parse(EnvironmentExtensions.GetVariable(MAX_CONCURRENT_RESOLVE));
						return new DomainLookupService(logger, dnsServer, parallelCount);
					});
					services.AddTransient<DomainPublishMessageHandler>();
				},
				beforeHostStartAction: services =>
				{
					var _messageQueue = services.GetRequiredService<IMessageQueue>();
					var handler = services.GetRequiredService<DomainPublishMessageHandler>();
					var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_DNS_DOMAINS_QUEUE);
					_messageQueue.Subscribe<DomainPublishMessage, DomainPublishMessageHandler>(queueName, handler);
				});
		}
	}
}
