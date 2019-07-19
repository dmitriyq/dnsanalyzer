using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.Models
{
	public class SuspectDomains
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public string Ip { get; set; }
	}
}
