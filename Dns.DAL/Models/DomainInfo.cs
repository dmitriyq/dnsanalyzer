using System;
using System.Collections.Generic;

namespace Dns.DAL.Models
{
	public class DomainInfo
	{
		public DomainInfo()
		{
			DomainNS = new HashSet<DomainNSs>();
		}

		public int Id { get; set; }
		public string Domain { get; set; }
		public string Company { get; set; }
		public string Registrant { get; set; }
		public DateTimeOffset? DateCreate { get; set; }
		public DateTimeOffset? DateUntil { get; set; }

		public virtual ICollection<DomainNSs> DomainNS { get; set; }
	}
}