using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dns.Contracts.Messages;

namespace Dns.Resolver.Aggregator.Models
{
	public class DomainQueueCollection
	{
		private readonly Queue<Guid> _uniqueIds = new Queue<Guid>();
		private readonly List<DomainResolvedMessage> _domains = new List<DomainResolvedMessage>();
		private readonly object _domainsLock = new object();

		public event EventHandler<UniqueIdCountChangedArgs>? NewIdAdded;

		public void Add(DomainResolvedMessage domain)
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
		public IEnumerable<DomainResolvedMessage> DequeueDomains()
		{
			var id = _uniqueIds.Dequeue();
			var domains = _domains.Where(x => x.Id == id).ToArray();
			lock (_domainsLock)
			{
				_domains.RemoveAll(x => x.Id == id);
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
