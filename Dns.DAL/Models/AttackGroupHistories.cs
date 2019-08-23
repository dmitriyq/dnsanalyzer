using System;
using System.ComponentModel.DataAnnotations.Schema;
using Dns.DAL.Enums;

namespace Dns.DAL.Models
{
	public class AttackGroupHistories
	{
		public int Id { get; set; }
		public DateTimeOffset Date { get; set; }
		public int PrevStatus { get; set; }
		public int CurrentStatus { get; set; }

		[NotMapped]
		public AttackGroupStatusEnum PrevStatusEnum => (AttackGroupStatusEnum)PrevStatus;

		[NotMapped]
		public AttackGroupStatusEnum CurrentEnum => (AttackGroupStatusEnum)CurrentStatus;

		public int AttackGroupId { get; set; }
		public virtual AttackGroups AttackGroup { get; set; }
	}
}