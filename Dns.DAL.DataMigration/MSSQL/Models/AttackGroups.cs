using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Dns.DAL.DataMigration.MSSQL.Enums;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class AttackGroups
	{
		public AttackGroups()
		{
			Attacks = new HashSet<Attacks>();
			Notes = new HashSet<AttackNotes>();
			GroupHistories = new HashSet<AttackGroupHistories>();
		}

		public int Id { get; set; }
		public DateTime DateBegin { get; set; }
		public DateTime? DateClose { get; set; }

		public int Status { get; set; }

		[NotMapped]
		public AttackGroupStatusEnum StatusEnum => (AttackGroupStatusEnum)Status;

		public virtual ICollection<Attacks> Attacks { get; set; }
		public virtual ICollection<AttackNotes> Notes { get; set; }
		public virtual ICollection<AttackGroupHistories> GroupHistories { get; set; }
	}
}
