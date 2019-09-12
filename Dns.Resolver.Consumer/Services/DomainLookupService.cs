using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

		public DomainLookupService(ILogger<DomainLookupService> logger, string dnsServer)
		{
			_logger = logger;
			_idnMapping = new IdnMapping();
			_dnsClient = new DnsClient(System.Net.Dns.GetHostAddresses(dnsServer).First());
		}

		public async Task<ISet<string>> GetIpAddressesAsync(string domain)
		{
			try
			{
				var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
				return resp.Select(x => x.ToString()).ToHashSet();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
				try
				{
					var resp = await _dnsClient.Lookup(_idnMapping.GetAscii(domain)).ConfigureAwait(false);
					return resp.Select(x => x.ToString()).ToHashSet();
				}
				catch (Exception ex1)
				{
					_logger.LogWarning(ex1, ex1.Message);
					throw;
				}
			}
		}
	}
}
