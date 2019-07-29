using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dns.Site.Models
{
	public class AttackEditManyParams
	{
		public List<int> Ids { get; set; } = new List<int>();
		public int Status { get; set; }
		public string Comment { get; set; }
	}
}
