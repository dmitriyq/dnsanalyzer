using System;

namespace Dns.DAL.Models
{
	public class AttackNotes
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public DateTimeOffset Date { get; set; }

		public int AttackGroupId { get; set; }
		public virtual AttackGroups AttackGroup { get; set; }
	}
}