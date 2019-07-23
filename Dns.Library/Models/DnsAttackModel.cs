using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Library.Models
{
	public class DnsAttackModel
	{
		public string BlackDomain { get; set; }
		public string WhiteDomain { get; set; }
		public string Ip { get; set; }
	}
}
