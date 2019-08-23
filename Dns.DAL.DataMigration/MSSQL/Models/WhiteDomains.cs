using System;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class WhiteDomains
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public DateTime DateAdded { get; set; }
	}
}