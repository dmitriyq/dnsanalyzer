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
	}
}
