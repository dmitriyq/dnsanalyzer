using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Dns.Contracts.Protobuf
{
	[ProtoContract]
	public class ResolvedDomain
	{
		[ProtoMember(1)]
		public string Name { get; set; }

		[ProtoMember(2)]
		public HashSet<string> IPAddresses { get; set; }

		protected ResolvedDomain()
		{
			Name = string.Empty;
			IPAddresses = new HashSet<string>();
		}

		public ResolvedDomain(string name, HashSet<string> ips)
		{
			Name = name;
			IPAddresses = ips;
		}
	}
}
