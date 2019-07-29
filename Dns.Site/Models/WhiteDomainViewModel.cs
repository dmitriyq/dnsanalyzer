using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dns.Site.Models
{
	public class WhiteDomainViewModel
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public DateTimeOffset DateAdded { get; set; }
	}
}
