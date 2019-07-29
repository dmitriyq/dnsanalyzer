using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dns.Site.Models
{
	public class SuspectDomainViewModel
	{
		public string Domain { get; set; }
		public List<IpInfo> Ips { get; set; } = new List<IpInfo>();
		public class IpInfo
		{
			public string Ip { get; set; }
			public string Subnet { get; set; }
			public string Company { get; set; }
			public string Country { get; set; }
		}
	}
}
