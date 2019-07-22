using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Dns.Library;
using Dns.Library.Models;
using DNS.Client;
using Grfc.Library.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Services
{
	public class ResolveService: IDisposable
	{
		private IdnMapping _idnMapping = new IdnMapping();
		private readonly ILogger<ResolveService> _logger;
		private readonly string _dnsServer;

		public ResolveService(ILogger<ResolveService> logger)
		{
			_logger = logger;
			_dnsServer = EnvironmentExtensions.GetVariable(EnvVars.HYPERLOCAL_SERVER);
		}

		public async Task<IEnumerable<ResolvedDomain>> ResolveDomainsWithRetry(IEnumerable<string> domains, int retryCount)
		{
			try
			{
				var shuffleDomain = domains.OrderBy(x => Guid.NewGuid());
				var resolvedDomains = await ResolveDomains(shuffleDomain);
				resolvedDomains = await ResolveRepeat(resolvedDomains, retryCount);
				_logger.LogInformation($"[ResolveDomains] Complete ({resolvedDomains.Count()} of {domains.Count()})");
				return resolvedDomains.Where(x => x.IsResolved).AsEnumerable();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<ResolvedDomain>> ResolveDomains(IEnumerable<string> domains)
		{
			var results = new List<ResolvedDomain>(domains.Count());

			var resolveBlock = new TransformBlock<string, ResolvedDomain>(d => Resolve(d),
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2000 });
			var bufferBlock = new BatchBlock<ResolvedDomain>(500);
			var insertBlock = new ActionBlock<ResolvedDomain[]>(domMod => results.AddRange(domMod));
			resolveBlock.LinkTo(bufferBlock, new DataflowLinkOptions { PropagateCompletion = true });
			bufferBlock.LinkTo(insertBlock, new DataflowLinkOptions { PropagateCompletion = true });

			foreach (var dom in domains)
			{
				await resolveBlock.SendAsync(dom);
			}
			resolveBlock.Complete();
			await insertBlock.Completion;
			return results;
		}

		public async Task<IEnumerable<ResolvedDomain>> ResolveRepeat(IEnumerable<ResolvedDomain> domains, int maxCount = 3, int tryCount = 0)
		{
			if (domains.Count(x => !x.IsResolved) == 0)
				return domains;

			var initList = domains.ToList();
			while (tryCount < maxCount)
			{
				var unResolved = initList.Where(x => !x.IsResolved).Select(x => x.Name);
				initList = initList.Where(x => x.IsResolved).ToList();
				var resolveStep = await ResolveDomains(unResolved);
				initList.AddRange(resolveStep);
				return await ResolveRepeat(initList, maxCount, ++tryCount);
			}
			_logger.LogInformation($"[ResolveDomains] ERRORS: {domains.Count(x => !x.IsResolved)} of {domains.Count()}. try: {tryCount}");
			return initList;
		}

		private async Task<ResolvedDomain> Resolve(string domain)
		{
			var servers = await System.Net.Dns.GetHostAddressesAsync(_dnsServer);
			var dnsServerIp = servers.First();
			var mdl = new ResolvedDomain() { Name = domain, IsResolved = false };
			try
			{
				var client = new DnsClient(dnsServerIp);
				var resp = await client.Lookup(_idnMapping.GetAscii(domain));
				mdl.IsResolved = true;
				mdl.IPAddresses = resp.Select(x => x.ToString()).ToHashSet();
				return mdl;
			}
			catch (Exception)
			{
				//Logger.Log($"[Resolve] fail {domain}", ex, LogType.INFO);
				return mdl;
			}
		}

		public void Dispose()
		{
			_idnMapping = null;
		}
	}
}
