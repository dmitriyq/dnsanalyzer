using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DNS.Client;
using DNS.Protocol;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Whitelist.Services
{
	public class DomainLookupService : IDomainLookupService
	{
		private readonly IdnMapping _idnMapping;
		private readonly ILogger<DomainLookupService> _logger;
		private readonly DnsClient _dnsClient;
		private readonly int _maxParrallelRequests;

		public DomainLookupService(ILogger<DomainLookupService> logger, string dnsServer, int maxParrallelRequests)
		{
			_logger = logger;
			_idnMapping = new IdnMapping();
			_dnsClient = new DnsClient(System.Net.Dns.GetHostAddresses(dnsServer).First());
			_maxParrallelRequests = maxParrallelRequests;
		}

		private async Task<(ISet<string> ips, ResponseCode code)> TryResolveWithRetry(string domain, int retryCount = 0)
		{
			while (retryCount < 3)
			{
				try
				{
					var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
					var ips = resp.Select(x => x.ToString()).ToHashSet();
					return (ips, code: ResponseCode.NoError);
				}
				catch (OperationCanceledException)
				{
					retryCount++;
				}
				catch { throw; }
			}
			_logger.LogWarning($"Timeout for {domain} [{retryCount}]");
			return (ips: new HashSet<string>(), code: ResponseCode.NotImplemented);
		}

		private async Task<(ISet<string> ips, ResponseCode code)> HandleResolveError(string domain)
		{
			try
			{
				return await TryResolveWithRetry(domain).ConfigureAwait(false);
			}
			catch (ResponseException re) when (re.Response.ResponseCode == ResponseCode.NameError
				|| re.Response.ResponseCode == ResponseCode.ServerFailure
				|| re.Message == "No matching records")
			{
				return (ips: new HashSet<string>(), code: re.Response.ResponseCode);
			}
		}

		public async Task<(ISet<string> ips, ResponseCode code)> GetIpAddressesAsync(string domain)
		{
			try
			{
				var result = await HandleResolveError(domain).ConfigureAwait(false);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, ex.Message);
				throw;
			}
		}

		private async Task<(string domain, ISet<string> ips, ResponseCode code)> GetIpAddressesWithDomainAsync(string domain)
		{
			try
			{
				var (ips, code) = await HandleResolveError(domain).ConfigureAwait(false);
				return (domain, ips, code);
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, ex.Message);
				throw;
			}
		}

		public async Task<IEnumerable<(string domain, ISet<string> ips, ResponseCode code)>> GetIpAddressesAsync(IEnumerable<string> domains)
		{
			var result = new List<(string domain, ISet<string> ips, ResponseCode code)>(domains.Count());

			var inputBuffer = new BufferBlock<string>();
			var resolveBlock = new TransformBlock<string, (string domain, ISet<string> ips, ResponseCode code)>(d =>
				GetIpAddressesWithDomainAsync(d), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxParrallelRequests });
			var addResultBlock = new ActionBlock<(string domain, ISet<string> ips, ResponseCode code)>(r => result.Add(r));

			var linkOpt = new DataflowLinkOptions { PropagateCompletion = true };

			inputBuffer.LinkTo(resolveBlock, linkOpt);
			resolveBlock.LinkTo(addResultBlock, linkOpt);

			foreach (var domain in domains)
			{
				await inputBuffer.SendAsync(domain).ConfigureAwait(false);
			}
			inputBuffer.Complete();

			await addResultBlock.Completion.ConfigureAwait(false);

			return result;
		}

		public bool IsSuccessResolve((ISet<string> ips, ResponseCode code) resolve) =>
			resolve.ips.Count > 0 && resolve.code == ResponseCode.NoError;

		public bool IsSuccessResolve((string domain, ISet<string> ips, ResponseCode code) resolve) =>
			resolve.ips.Count > 0 && resolve.code == ResponseCode.NoError;
	}
}
