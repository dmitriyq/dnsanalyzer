using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Analyzer.Models
{
	public class AttackModel
	{
		public string BlackDomain { get; }
		public string WhiteDomain { get; }
		public string Ip { get; }

		public AttackModel(string blackDomain, string whiteDomain, string ip)
		{
			BlackDomain = blackDomain;
			WhiteDomain = whiteDomain;
			Ip = ip;
		}
	}
}
