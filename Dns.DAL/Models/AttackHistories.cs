using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Dns.DAL.Enums;

namespace Dns.DAL.Models
{
	public class AttackHistories
	{
		public int Id { get; set; }
		public DateTimeOffset Date { get; set; }
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
