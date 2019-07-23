using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.DataMigration.MSSQL.Models
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
		public DateTime? DateCreate { get; set; }
		public DateTime? DateUntil { get; set; }

		public virtual ICollection<DomainNSs> DomainNS { get; set; }
	}
}
