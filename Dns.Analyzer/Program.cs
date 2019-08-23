using System;
using System.Threading;
using System.Threading.Tasks;
using Dns.Analyzer.Services;
using Dns.DAL;
using Dns.Library;
using Dns.Library.Services;
using Grfc.Library.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dns.Analyzer
{
	public static class Program
	{
		public static ManualResetEventSlim resetEventSlim = new ManualResetEventSlim();

		public static void Main(string[] args)
		{
			EnvironmentExtensions.CheckVariables(
				EnvVars.PG_CONNECTION_STRING_READ,
				EnvVars.PG_CONNECTION_STRING_WRITE,
				EnvVars.REDIS_CONNECTION,
				EnvVars.ANALYZER_SUSPECT_IP_COUNT,
				EnvVars.NOTIFICATION_EMAIL_FROM
				);

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			var services = serviceCollection.BuildServiceProvider();
			ApplyMigrations(services);

			var bootstrapper = services.GetService<Bootstrapper>();
			Task.Run(async () =>
			{
				await bootstrapper.StartAnalyzer();
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

			services.AddTransient<AttackService>();
			services.AddTransient<SuspectDomainSevice>();
			services.AddTransient<IpInfoService>();
			services.AddTransient<Bootstrapper>();
			services.AddTransient<NotifyService>();

			services.AddSingleton<RedisService>();
		}

		private static void ApplyMigrations(IServiceProvider services)
		{
			var cntx = services.GetService<DnsDbContext>();
			cntx.Database.Migrate();
		}
	}
}