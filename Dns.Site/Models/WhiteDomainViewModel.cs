using System;

namespace Dns.Site.Models
{
	public class WhiteDomainViewModel
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public DateTimeOffset DateAdded { get; set; }
	}
}