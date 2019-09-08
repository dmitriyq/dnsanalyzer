using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Analyzer.Models
{
	public class RedisKeys
	{
		public string BlackDomainsResolvedKey { get; }
		public string WhiteDomainsResolvedKey { get; }
		public string VigruzkiIpKey { get; }
		public string VigruzkiSubnetKey { get; }
		public string NotificationSendMessageKey { get; }

		public RedisKeys(string blackDomainsResolvedKey, string whiteDomainsResolvedKey, string vigruzkiIpKey, string vigruzkiSubnetKey, string notifyMessageKey)
		{
			BlackDomainsResolvedKey = blackDomainsResolvedKey;
			WhiteDomainsResolvedKey = whiteDomainsResolvedKey;
			VigruzkiIpKey = vigruzkiIpKey;
			VigruzkiSubnetKey = vigruzkiSubnetKey;
			NotificationSendMessageKey = notifyMessageKey;
		}
	}
}
