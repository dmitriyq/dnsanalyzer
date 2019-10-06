using System;
using Dns.DAL;
using Dns.Resolver.Whitelist.Services;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dns.Resolver.Whitelist
{
	public sealed class Program
	{
		public const string HYPERLOCAL_SERVER = nameof(HYPERLOCAL_SERVER);
		public const string MAX_CONCURRENT_RESOLVE = nameof(MAX_CONCURRENT_RESOLVE);
		public const string REFRESH_INTERVAL = nameof(REFRESH_INTERVAL);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);

		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public static void Main(string[] args)
		{
			Grfc.Library.Common.Extensions.ServiceCollectionExtensions.StartAsConsoleApplication<Program>(
				entryPointArgs: args,
				requiredEnvVars: new string[]
				{
					REDIS_CONNECTION,
					PG_CONNECTION_STRING_WRITE,
					PG_CONNECTION_STRING_READ,
					RABBITMQ_CONNECTION,
					HYPERLOCAL_SERVER,
					MAX_CONCURRENT_RESOLVE,
					REFRESH_INTERVAL,
				},
				configureServices: (_, services) =>
				{
					services.AddOptions();
					services.AddLogging(l => l.AddConsole()
						.SetEFCoreLogLevel(LogLevel.Warning));

					services.AddSingleton<ConnectionMultiplexer>(__ =>
						ConnectionMultiplexer.Connect(EnvironmentExtensions.GetVariable(REDIS_CONNECTION)));

					services.AddStackExchangeRedisCache(opts =>
					{
						opts.Configuration = EnvironmentExtensions.GetVariable(REDIS_CONNECTION);
						opts.InstanceName = typeof(Dns.Resolver.Whitelist.Program).Namespace;
					});

					services.AddEasyNetQ(EnvironmentExtensions.GetVariable(RABBITMQ_CONNECTION));

					services.AddDbContext<DnsDbContext>(opt =>
						opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));
					services.AddDbContext<DnsReadOnlyDbContext>(opt =>
						opt.UseNpgsql(EnvironmentExtensions.GetVariable(PG_CONNECTION_STRING_READ)));

					services.AddTransient<IDomainLookupService, DomainLookupService>(sp =>
					{
						var logger = sp.GetRequiredService<ILogger<DomainLookupService>>();
						var dnsServer = EnvironmentExtensions.GetVariable(HYPERLOCAL_SERVER);
						var parrallelRequests = int.Parse(EnvironmentExtensions.GetVariable(MAX_CONCURRENT_RESOLVE));
						return new DomainLookupService(logger, dnsServer, parrallelRequests);
					});
					services.AddTransient<IWhitelistService, WhitelistService>();
					services.AddTransient<IWhitelistUpdateService, WhitelistUpdateService>(sp =>
					{
						var lookup = sp.GetRequiredService<IDomainLookupService>();
						var whitelist = sp.GetRequiredService<IWhitelistService>();
						var logger = sp.GetRequiredService<ILogger<WhitelistUpdateService>>();
						var messageQueue = sp.GetRequiredService<IMessageQueue>();
						var refreshInterval = TimeSpan.FromSeconds(int.Parse(EnvironmentExtensions.GetVariable(REFRESH_INTERVAL)));
						return new WhitelistUpdateService(lookup, whitelist, logger, messageQueue, refreshInterval);
					});
				},
				beforeHostStartAction: services =>
				{
					services.ApplyDbMigration<DnsDbContext>();

					var svc = services.GetRequiredService<IWhitelistUpdateService>();
					_ = svc.RunJobAsync();
				});
		}
	}
}
