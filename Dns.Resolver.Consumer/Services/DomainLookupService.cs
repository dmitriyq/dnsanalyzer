using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DNS.Client;
using DNS.Protocol;
using Microsoft.Extensions.Logging;

namespace Dns.Resolver.Consumer.Services
{
	public class DomainLookupService : IDomainLookupService
	{
		private readonly IdnMapping _idnMapping;
		private readonly ILogger<DomainLookupService> _logger;
		private readonly DnsClient _dnsClient;
		private readonly SemaphoreSlim _semaphoreSlim;

		public DomainLookupService(ILogger<DomainLookupService> logger, string dnsServer, int maxParrallelRequests)
		{
			_logger = logger;
			_idnMapping = new IdnMapping();
			_dnsClient = new DnsClient(System.Net.Dns.GetHostAddresses(dnsServer).First());
			_semaphoreSlim = new SemaphoreSlim(maxParrallelRequests);
		}

		private async Task<(ISet<string> ips, ResponseCode code)> GetIpsAsync(string domain)
		{
			var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
			var ips = resp.Select(x => x.ToString()).ToHashSet();
			return (ips: ips, code: ResponseCode.NoError);
		}

		private async Task<(ISet<string> ips, ResponseCode code)> HandleResolveError(string domain)
		{
			try
			{
				return await GetIpsAsync(domain).ConfigureAwait(false);
			}
			catch (ResponseException re) when (re.Response.ResponseCode == ResponseCode.NameError
				|| re.Response.ResponseCode == ResponseCode.ServerFailure
				|| re.Message == "No matching records")
			{
				return (ips: new HashSet<string>(), code: re.Response.ResponseCode);
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning($"OperationCanceledException for {domain}");
				return await GetIpsAsync(domain).ConfigureAwait(false);
			}
		}

		public async Task<(ISet<string> ips, ResponseCode code)> GetIpAddressesAsync(string domain)
		{
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				var result = await HandleResolveError(domain).ConfigureAwait(false);
				_semaphoreSlim.Release();
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex, ex.Message);
				_semaphoreSlim.Release();
				throw;
			}
		}
	}
}
