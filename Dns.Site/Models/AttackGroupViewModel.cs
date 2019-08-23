using System.Collections.Generic;
using System.Linq;

namespace Dns.Site.Models
{
	public class AttackGroupViewModel
	{
		public int Id { get; set; }
		public int Status { get; set; }
		public string WhiteDomain { get; set; }
		public string BlackDomain { get; set; }

		public int TotalCount => Summary.Sum(x => x.Count);
		public List<AttacksSummaryViewModel> Summary { get; set; } = new List<AttacksSummaryViewModel>();

		public class AttacksSummaryViewModel
		{
			public int Status { get; set; }
			public int Count { get; set; }
		}
	}
}