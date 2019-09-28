using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Contracts.Messages
{
	public interface ITraceable
	{
		public Guid TraceId { get; set; }
	}
}
