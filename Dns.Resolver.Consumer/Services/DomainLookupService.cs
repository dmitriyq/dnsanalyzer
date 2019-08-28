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
				_logger.LogInformation($"Resolve success for {domain} - {string.Join(", ", resp)}");
				return resp.Select(x => x.ToString()).ToHashSet();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, ex.Message);
				throw;
			}
		}
	}
}
