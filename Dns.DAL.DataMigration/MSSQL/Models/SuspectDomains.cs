namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class SuspectDomains
	{
		public int Id { get; set; }
		public string Domain { get; set; }
		public string Ip { get; set; }
	}
}