using System;
using Dns.Contracts.Messages;
using Dns.Resolver.Aggregator.Messages;
using Dns.Resolver.Aggregator.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
using Grfc.Library.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Dns.Resolver.Aggregator
{
	public sealed class Program
	{
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_BLACK_DOMAIN_RESOLVED = nameof(REDIS_BLACK_DOMAIN_RESOLVED);
		public const string REDIS_WHITE_DOMAIN_RESOLVED = nameof(REDIS_WHITE_DOMAIN_RESOLVED);

		public static void Main(string[] args)
		{
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new[]
				{
					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE,
					REDIS_CONNECTION,
					REDIS_BLACK_DOMAIN_RESOLVED,
					REDIS_WHITE_DOMAIN_RESOLVED
				},
				configureServices: (_, services) =>
				{
					services.AddOptions();
					services.AddLogging(l => l.AddConsole()
						.SetEFCoreLogLevel(LogLevel.Warning));

					services.AddSingleton<ConnectionMultiplexer>(__ =>
					{
						var redisConnection = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
						return ConnectionMultiplexer.Connect(redisConnection);
					});

					services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

					services.AddSingleton<IDomainAggregatorService, DomainAggregatorService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<DomainAggregatorService>>();
						var redis = sp.GetRequiredService<ConnectionMultiplexer>();
						var messageQueue = sp.GetRequiredService<IMessageQueue>();

						var blackDomainKey = EnvironmentExtensions.GetVariable(REDIS_BLACK_DOMAIN_RESOLVED);
						var whiteDomainKey = EnvironmentExtensions.GetVariable(REDIS_WHITE_DOMAIN_RESOLVED);

						return new DomainAggregatorService(logger, redis, messageQueue, blackDomainKey, whiteDomainKey);
					});

					services.AddTransient<DomainResolvedMessageHandler>();
				},
				beforeHostStartAction: services =>
				{
					var _messageQueue = services.GetRequiredService<IMessageQueue>();
					var handler = services.GetRequiredService<DomainResolvedMessageHandler>();
					var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE);
					_messageQueue.Subscribe<DomainResolvedMessage, DomainResolvedMessageHandler>(queueName, handler);
				});
		}
	}
}
