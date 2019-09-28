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

		public async Task<(ISet<string> ips, ResponseCode code)> GetIpAddressesAsync(string domain)
		{
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
				var ips = resp.Select(x => x.ToString()).ToHashSet();
				_semaphoreSlim.Release();
				return (ips: ips, code: ResponseCode.NoError);
			}
			catch (ResponseException re) when (re.Response.ResponseCode == ResponseCode.NameError
				|| re.Response.ResponseCode == ResponseCode.ServerFailure)
			{
				_semaphoreSlim.Release();
				return (ips: new HashSet<string>(), code: re.Response.ResponseCode);
			}
			catch (Exception ex)
			{
				try
				{
					var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
					var ips = resp.Select(x => x.ToString()).ToHashSet();
					_semaphoreSlim.Release();
					return (ips: ips, code: ResponseCode.NoError);
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
