using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dns.Site.Models
{
	public class AttackEditParams
	{
		public int Id { get; set; }
		public int Status { get; set; }
		public string Comment { get; set; }
	}
}
