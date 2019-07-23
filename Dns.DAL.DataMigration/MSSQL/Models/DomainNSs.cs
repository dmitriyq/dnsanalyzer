using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class DomainNSs
	{
		public int Id { get; set; }

		public int DomainId { get; set; }
		public virtual DomainInfo Domain { get; set; }

		public int NsId { get; set; }
		public virtual NameServers NameServer { get; set; }
	}
}
