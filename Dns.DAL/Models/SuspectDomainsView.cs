using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.Models
{
	public class SuspectDomainsView
	{
		public string Domain { get; set; }
		public string Ip { get; set; }
		public string Company { get; set; }
		public string Country { get; set; }
		public string Subnet { get; set; }
	}
}
