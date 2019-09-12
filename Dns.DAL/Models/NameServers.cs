using System.Collections.Generic;

namespace Dns.DAL.Models
{
	public class NameServers
	{
		public NameServers()
		{
			DomainNS = new HashSet<DomainNSs>();
		}

		public int Id { get; set; }
		public string NameServer { get; set; }

		public virtual ICollection<DomainNSs> DomainNS { get; set; }
	}
}