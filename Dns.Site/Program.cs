using Dns.Library;
using Grfc.Library.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Dns.Site
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			EnvironmentExtensions.CheckVariables(
				EnvVars.AUTH_SERVER_URL,
				EnvVars.NOTIFY_SERVICE_URL,
				EnvVars.VIGRUZKI_SERVICE_URL,
				EnvVars.REDIS_CONNECTION,
				EnvVars.PG_CONNECTION_STRING_READ,
				EnvVars.NOTIFICATION_EMAIL_FROM,
				EnvVars.PG_CONNECTION_STRING_WRITE);

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}