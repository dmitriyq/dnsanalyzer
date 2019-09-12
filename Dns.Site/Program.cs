using Grfc.Library.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Dns.Site
{
	public static class Program
	{
		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);
		public const string AUTH_SERVER_URL = nameof(AUTH_SERVER_URL);
		public const string NOTIFY_SERVICE_URL = nameof(NOTIFY_SERVICE_URL);
		public const string VIGRUZKI_SERVICE_URL = nameof(VIGRUZKI_SERVICE_URL);
		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
		public const string NOTIFY_SEND_CHANNEL = nameof(NOTIFY_SEND_CHANNEL);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);

		public static void Main(string[] args)
		{
			EnvironmentExtensions.CheckVariables(
				PG_CONNECTION_STRING_WRITE,
				PG_CONNECTION_STRING_READ,
				AUTH_SERVER_URL,
				NOTIFY_SERVICE_URL,
				VIGRUZKI_SERVICE_URL,
				REDIS_CONNECTION,
				NOTIFY_SEND_CHANNEL,
				NOTIFICATION_EMAIL_FROM);

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
	}
}