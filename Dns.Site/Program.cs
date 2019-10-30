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
		public const string SITE_HOSTNAME = nameof(SITE_HOSTNAME);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_HEALTH_QUEUE = nameof(RABBITMQ_HEALTH_QUEUE);
		public const string RABBITMQ_ATTACK_UPDATE_QUEUE = nameof(RABBITMQ_ATTACK_UPDATE_QUEUE);

		public const string DISABLE_AUTH = nameof(DISABLE_AUTH);

		public static void Main(string[] args)
		{
			EnvironmentExtensions.CheckVariables(
				PG_CONNECTION_STRING_WRITE,
				PG_CONNECTION_STRING_READ,
				AUTH_SERVER_URL,
				NOTIFY_SERVICE_URL,
				VIGRUZKI_SERVICE_URL,
				REDIS_CONNECTION,
				NOTIFICATION_EMAIL_FROM,
				SITE_HOSTNAME,
				RABBITMQ_CONNECTION,
				RABBITMQ_HEALTH_QUEUE,
				RABBITMQ_ATTACK_UPDATE_QUEUE,
				DISABLE_AUTH);

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
	}
}