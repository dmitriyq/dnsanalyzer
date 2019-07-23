using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.DAL.DataMigration.MSSQL.Models
{
	public class AttackNotes
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public DateTime Date { get; set; }

		public int AttackGroupId { get; set; }
		public virtual AttackGroups AttackGroup { get; set; }
	}
}
