namespace Dns.Library
{
	public static class EnvVars
	{
		public const string AUTH_SERVER_URL = nameof(AUTH_SERVER_URL);
		public const string NOTIFY_SERVICE_URL = nameof(NOTIFY_SERVICE_URL);
		public const string VIGRUZKI_SERVICE_URL = nameof(VIGRUZKI_SERVICE_URL);

		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);
		public const string HYPERLOCAL_SERVER = nameof(HYPERLOCAL_SERVER);

		public const string RESOLVER_MAX_DEGREE_OF_PARALLELISM = nameof(RESOLVER_MAX_DEGREE_OF_PARALLELISM);
		public const string RESOLVER_BUFFER_BLOCK_SIZE = nameof(RESOLVER_BUFFER_BLOCK_SIZE);

		public const string ANALYZER_SUSPECT_IP_COUNT = nameof(ANALYZER_SUSPECT_IP_COUNT);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);

		public const string RABBITMQ_CONNECTION = nameof(RABBITMQ_CONNECTION);
		public const string RABBITMQ_DNS_DOMAINS_QUEUE = nameof(RABBITMQ_DNS_DOMAINS_QUEUE);
		public const string RABBITMQ_DNS_DOMAINS_AGGREGATOR_QUEUE = nameof(RABBITMQ_DNS_DOMAINS_AGGREGATOR_QUEUE);

		public const string REDIS_BLACK_DOMAINS = nameof(REDIS_BLACK_DOMAINS);

		public const string RESOLVER_PUBLISHER_DELAY_SEC = nameof(RESOLVER_PUBLISHER_DELAY_SEC);
	}
}