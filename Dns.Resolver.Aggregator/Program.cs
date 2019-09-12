using System;
using Dns.Contracts.Messages;
using Dns.Resolver.Aggregator.Messages;
using Dns.Resolver.Aggregator.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
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
#pragma warning disable CA1707 // Идентификаторы не должны содержать символы подчеркивания
		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE);
		public const string RABBITMQ_ANALYZE_QUEUE = nameof(RABBITMQ_ANALYZE_QUEUE);
		public const string RABBITMQ_HEALTH_QUEUE = nameof(RABBITMQ_HEALTH_QUEUE);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string REDIS_BLACK_DOMAIN_RESOLVED = nameof(REDIS_BLACK_DOMAIN_RESOLVED);
		public const string REDIS_WHITE_DOMAIN_RESOLVED = nameof(REDIS_WHITE_DOMAIN_RESOLVED);
#pragma warning restore CA1707 // Идентификаторы не должны содержать символы подчеркивания

		public static void Main(string[] args)
		{
			ILogger<Program>? _logger = null;
			IHost? host = null;

			try
			{
				EnvironmentExtensions.CheckVariables(
					RABBITMQ_CONNECTION,
					RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE,
					REDIS_CONNECTION,
					REDIS_BLACK_DOMAIN_RESOLVED,
					REDIS_WHITE_DOMAIN_RESOLVED
				);

				host = CreateHostBuilder(args);
				_logger = host.Services.GetRequiredService<ILogger<Program>>();

				var _messageQueue = host.Services.GetRequiredService<IMessageQueue>();
				var handler = host.Services.GetRequiredService<DomainResolvedMessageHandler>();
				_messageQueue.HandleMessage<DomainResolvedMessage, DomainResolvedMessageHandler>(handler, EnvironmentExtensions.GetVariable(RABBITMQ_DNS_RESOLVED_DOMAINS_QUEUE));

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
				services.AddSingleton<IDomainAggregatorService, DomainAggregatorService>(sp =>
				{
					var logger = sp.GetRequiredService<ILogger<DomainAggregatorService>>();
					var redis = sp.GetRequiredService<ConnectionMultiplexer>();
					var messageQueue = sp.GetRequiredService<IMessageQueue>();
					var queueName = EnvironmentExtensions.GetVariable(RABBITMQ_ANALYZE_QUEUE);
					var healthQueue = EnvironmentExtensions.GetVariable(RABBITMQ_HEALTH_QUEUE);

					var blackDomainKey = EnvironmentExtensions.GetVariable(REDIS_BLACK_DOMAIN_RESOLVED);
					var whiteDomainKey = EnvironmentExtensions.GetVariable(REDIS_WHITE_DOMAIN_RESOLVED);

					return new DomainAggregatorService(logger, redis, messageQueue, blackDomainKey, whiteDomainKey, queueName, healthQueue);
				});

				services.AddTransient<DomainResolvedMessageHandler>();
			})
			.Build();
	}
}
