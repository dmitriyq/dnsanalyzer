namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class DomainExcludedNames
	{
		public int Id { get; set; }
		public string WhiteDomain { get; set; }
		public string BlackDomain { get; set; }
	}
}