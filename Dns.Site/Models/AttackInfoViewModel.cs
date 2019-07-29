using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dns.Site.Models
{
	public class AttackInfoViewModel
	{
		public int Id { get; set; }
		public int Status { get; set; }
		public string WhiteDomain { get; set; }
		public string BlackDomain { get; set; }
		public DateTimeOffset Begin { get; set; }
		public DateTimeOffset? Close { get; set; }

		public List<AttackIpViewModel> Attacks { get; set; } = new List<AttackIpViewModel>();
		public List<NoteViewModel> Notes { get; set; } = new List<NoteViewModel>();
		public List<History> Histories { get; set; } = new List<History>();

		public DomainViewModel BlackDomainInfo { get; set; }
		public DomainViewModel WhiteDomainInfo { get; set; }

		public class NoteViewModel
		{
			public int Id { get; set; }
			public DateTimeOffset Create { get; set; } = DateTimeOffset.UtcNow;
			public string Text { get; set; }
			public int AttackId { get; set; }
		}
		public class IpViewModel
		{
			public string Ip { get; set; }
			public string Subnet { get; set; }
			public string Company { get; set; }
			public string Country { get; set; }
		}
		public class DomainViewModel
		{
			public string Domain { get; set; }
			public string Company { get; set; }
			public string Registrant { get; set; }
			public DateTimeOffset? DateCreate { get; set; }
			public DateTimeOffset? DateUntil { get; set; }
			public List<string> NameServers { get; set; } = new List<string>();
		}

		public class AttackIpViewModel
		{
			public int Id { get; set; }
			public string Ip { get; set; }
			public bool IpBlocked { get; set; }
			public string SubnetBlocked { get; set; }
			public int Status { get; set; }

			public IpViewModel IpInfo { get; set; }
			public List<History> Histories { get; set; } = new List<History>();
		}
		public class History
		{
			public int Id { get; set; }
			public DateTimeOffset Create { get; set; }
			public int PrevStatus { get; set; }
			public int CurrentStatus { get; set; }
		}
	}
}
