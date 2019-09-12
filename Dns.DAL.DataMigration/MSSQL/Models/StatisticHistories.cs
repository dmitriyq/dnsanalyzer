using System;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class StatisticHistories
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public int GroupNew { get; set; }
		public int GroupThreat { get; set; }
		public int GroupDynamic { get; set; }
		public int GroupAttack { get; set; }
		public int GroupComplete { get; set; }
		public int GroupTotal { get; set; }

		public int AttackNew { get; set; }
		public int AttackIntersection { get; set; }
		public int AttackComplete { get; set; }
		public int AttackTotal { get; set; }
	}
}