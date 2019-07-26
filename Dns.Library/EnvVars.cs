using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Library
{
	public class EnvVars
	{
		public const string REDIS_CONNECTION = nameof(REDIS_CONNECTION);
		public const string PG_CONNECTION_STRING_WRITE = nameof(PG_CONNECTION_STRING_WRITE);
		public const string PG_CONNECTION_STRING_READ = nameof(PG_CONNECTION_STRING_READ);
		public const string HYPERLOCAL_SERVER = nameof(HYPERLOCAL_SERVER);

		public const string RESOLVER_MAX_DEGREE_OF_PARALLELISM = nameof(RESOLVER_MAX_DEGREE_OF_PARALLELISM);
		public const string RESOLVER_BUFFER_BLOCK_SIZE = nameof(RESOLVER_BUFFER_BLOCK_SIZE);

		public const string ANALYZER_SUSPECT_IP_COUNT = nameof(ANALYZER_SUSPECT_IP_COUNT);

		public const string NOTIFICATION_EMAIL_FROM = nameof(NOTIFICATION_EMAIL_FROM);
	}
}
