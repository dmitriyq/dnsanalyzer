using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.Library;
using Dns.Library.Services;
using Dns.Resolver.Services;
using Grfc.Library.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver
{
	public class Program
	{
		public static ManualResetEventSlim resetEventSlim = new ManualResetEventSlim();

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
		public static void Main(string[] args)
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
#pragma warning restore RCS1163 // Unused parameter.
		{
			EnvironmentExtensions.CheckVariables(
				EnvVars.HYPERLOCAL_SERVER,
				EnvVars.PG_CONNECTION_STRING_READ,
				EnvVars.PG_CONNECTION_STRING_WRITE,
				EnvVars.REDIS_CONNECTION,
				EnvVars.RESOLVER_BUFFER_BLOCK_SIZE,
				EnvVars.RESOLVER_MAX_DEGREE_OF_PARALLELISM
				);

			ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
			ServicePointManager.DefaultConnectionLimit = int.MaxValue;

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			var services = serviceCollection.BuildServiceProvider();
			ApplyMigrations(services);

			var bootstrapper = services.GetService<Bootstrapper>();
			var logger = services.GetService<ILoggerFactory>().CreateLogger<Program>();
			Task.Run(async () =>
			{
				try
				{
					while (true)
					{
						await bootstrapper.ResolveDomains().ConfigureAwait(false);
						await bootstrapper.NotifyCompletion().ConfigureAwait(false);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, ex.Message);
					throw;
				}
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

			services.AddTransient<ResolveService>();
			services.AddSingleton<RedisService>();
			services.AddTransient<Bootstrapper>();
		}

		private static void ApplyMigrations(IServiceProvider services)
		{
			var cntx = services.GetService<DnsDbContext>();
			cntx.Database.Migrate();
		}
	}
}