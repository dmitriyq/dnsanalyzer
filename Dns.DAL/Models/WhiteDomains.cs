using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.Models
{
	public class WhiteDomains
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public DateTimeOffset DateAdded { get; set; }
	}
}
