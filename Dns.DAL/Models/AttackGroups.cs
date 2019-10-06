using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dns.DAL.Enums;

namespace Dns.DAL.Models
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
		public DateTimeOffset DateBegin { get; set; }
		public DateTimeOffset? DateClose { get; set; }

		public int Status { get; set; }

		public DateTimeOffset LastUpdate { get; set; }

		[NotMapped]
		public AttackGroupStatusEnum StatusEnum => (AttackGroupStatusEnum)Status;

		public virtual ICollection<Attacks> Attacks { get; set; }
		public virtual ICollection<AttackNotes> Notes { get; set; }
		public virtual ICollection<AttackGroupHistories> GroupHistories { get; set; }
	}
}