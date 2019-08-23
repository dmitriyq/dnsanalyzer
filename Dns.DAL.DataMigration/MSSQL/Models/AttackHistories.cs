using System;
using System.ComponentModel.DataAnnotations.Schema;
using Dns.DAL.DataMigration.MSSQL.Enums;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class AttackHistories
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public int PrevStatus { get; set; }
		public int CurrentStatus { get; set; }

		[NotMapped]
		public AttackStatusEnum PrevStatusEnum => (AttackStatusEnum)PrevStatus;

		[NotMapped]
		public AttackStatusEnum CurrentEnum => (AttackStatusEnum)CurrentStatus;

		public int AttackId { get; set; }
		public virtual Attacks Attack { get; set; }
	}
}