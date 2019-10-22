using System;
using System.Collections.Generic;
using System.Text;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Analyzer.Services.Interfaces
{
	public interface IBatchingService<T> where T: class
	{
		void Add(T message);
	}
}
