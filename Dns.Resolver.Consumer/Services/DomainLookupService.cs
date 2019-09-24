using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DNS.Client;
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

		public async Task<ISet<string>> GetIpAddressesAsync(string domain)
		{
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
				var ips = resp.Select(x => x.ToString()).ToHashSet();
				_semaphoreSlim.Release();
				return ips;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
				try
				{
					var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
					var ips = resp.Select(x => x.ToString()).ToHashSet();
					_semaphoreSlim.Release();
					return ips;
				}
				catch (Exception ex1)
				{
					_logger.LogWarning(ex1, ex1.Message);
					_semaphoreSlim.Release();
					throw;
				}
			}
		}
	}
}
