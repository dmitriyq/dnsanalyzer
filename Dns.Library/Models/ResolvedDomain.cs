using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Dns.Library.Models
{
	[ProtoContract]
	public class ResolvedDomain
	{
		[ProtoMember(1)]
		public string Name { get; set; }

		[ProtoMember(2)]
		public HashSet<string> IPAddresses { get; set; }

		[ProtoMember(3)]
		public HashSet<string> NameServers { get; set; }
	}
}
