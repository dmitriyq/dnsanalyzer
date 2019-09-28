using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Aggregator.Models
{
	public class DomainQueueCollection<T> where T: ITraceable
	{
		private readonly Queue<Guid> _uniqueIds = new Queue<Guid>();
		private readonly List<T> _domains = new List<T>();
		private readonly object _domainsLock = new object();

		public event EventHandler<UniqueIdCountChangedArgs>? NewIdAdded;

		public void Add(T domain)
		{
			if (domain == null) throw new ArgumentNullException(nameof(domain));

			if (!_uniqueIds.Contains(domain.TraceId))
			{
				_uniqueIds.Enqueue(domain.TraceId);
				NewIdAdded?.Invoke(this, new UniqueIdCountChangedArgs(_uniqueIds.Count, domain.TraceId));
			}
			lock (_domainsLock)
			{
				_domains.Add(domain);
			}
		}

		public IEnumerable<T> DequeueDomains()
		{
			var id = _uniqueIds.Dequeue();
			var domains = _domains.Where(x => x.TraceId == id).ToArray();
			lock (_domainsLock)
			{
				_domains.RemoveAll(x => x.TraceId == id);
			}
			return domains;
		}
	}

	public class UniqueIdCountChangedArgs
	{
		public int Count { get; }
		public Guid TraceId { get; }

		public UniqueIdCountChangedArgs(int count, Guid traceId)
		{
			Count = count;
			TraceId = traceId;
		}
	}
}
