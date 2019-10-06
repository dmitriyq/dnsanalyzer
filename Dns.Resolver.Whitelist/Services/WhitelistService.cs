using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Protobuf;
using Dns.DAL;
using Grfc.Library.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace Dns.Resolver.Whitelist.Services
{
	public class WhitelistService : IWhitelistService
	{
		private readonly DnsReadOnlyDbContext _readOnlyDbContext;
		private readonly IDistributedCache _cache;
		private const string _key = "whitelist";

		public WhitelistService(IDistributedCache cache, DnsReadOnlyDbContext readOnlyDbContext)
		{
			_cache = cache;
			_readOnlyDbContext = readOnlyDbContext;
		}

		public async Task AddOrUpdateDomainsAsync(IEnumerable<ResolvedDomain> domains)
		{
			var serialized = domains.ToArray().ProtoSerialize();
			await _cache.SetAsync(_key, serialized).ConfigureAwait(false);
		}

		public async Task<ResolvedDomain?> GetDomainAsync(string domain)
		{
			var cached = await _cache.GetAsync(_key).ConfigureAwait(false);
			return cached?.ProtoDeserialize<ResolvedDomain[]>().FirstOrDefault(x => x.Name == domain);
		}

		public Task<IEnumerable<string>> GetDomainNamesAsync() =>
			Task.FromResult(GetDomainsFromDb());

		public async Task<IEnumerable<ResolvedDomain>> GetDomainsAsync()
		{
			var cached = await _cache.GetAsync(_key).ConfigureAwait(false);
			return cached?.ProtoDeserialize<ResolvedDomain[]>() ?? new ResolvedDomain[0];
		}

		private IEnumerable<string> GetDomainsFromDb()
		{
			return _readOnlyDbContext.WhiteDomains
				.Select(x => x.Domain).ToHashSet();
		}
	}
}
