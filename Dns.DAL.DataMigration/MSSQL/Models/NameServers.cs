using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.DataMigration.MSSQL.Models
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
