using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dns.DAL.Enums;

namespace Dns.DAL.Models
{
	public class Attacks
	{
		public Attacks()
		{
			Histories = new HashSet<AttackHistories>();
		}

		public int Id { get; set; }
		public string WhiteDomain { get; set; }
		public string BlackDomain { get; set; }
		public string Ip { get; set; }
		public bool IpBlocked { get; set; }
		public string SubnetBlocked { get; set; }

		public int Status { get; set; }

		[NotMapped]
		public AttackStatusEnum StatusEnum => (AttackStatusEnum)Status;

		public virtual ICollection<AttackHistories> Histories { get; set; }

		public int AttackGroupId { get; set; }
		public virtual AttackGroups AttackGroup { get; set; }
	}
}