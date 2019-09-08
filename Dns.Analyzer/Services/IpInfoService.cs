using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Whois.NET;

namespace Dns.Analyzer.Services
{
	public class IpInfoService : IIpInfoService
	{
		private readonly ILogger<IpInfoService> _logger;
		private readonly DnsDbContext _dbContext;

		public IpInfoService(ILogger<IpInfoService> logger, DnsDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public IpInfo? GetInfo(string ip)
		{
			WhoisResponse? response = null;
			try
			{
				var query = WhoisClient.Query(ip);
				response = query;
				return ParseResponse(query, ip);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Parsing failed for ip {ip}");
				try
				{
					if (response?.OrganizationName != null)
					{
						var org = response.OrganizationName;
						var orgPassed = org;
						if (org.Contains("(") && org.Contains(")"))
						{
							var openBracket = org.LastIndexOf('(');
							var closeBracket = org.LastIndexOf(')');
							orgPassed = org.Substring(openBracket + 1, closeBracket - openBracket - 1);
						}

						var q1 = WhoisClient.Query(orgPassed, response.RespondedServers.Last());
						response = q1;
						return ParseResponse(q1, ip);
					}
				}
				catch (Exception iex)
				{
					_logger.LogWarning(iex, $"Ошибка при получении информации об IP: {ip}");
				}
			}
			return null;
		}

		public async Task UpdateIpInfoAsync(bool onlyUnresolved)
		{
			_logger.LogInformation("Starting update ip information");

			HashSet<string> ips = onlyUnresolved ?
				await UnResolvedIps().ConfigureAwait(false) :
				_dbContext.DnsAttacks.Select(x => x.Ip).Distinct().ToHashSet();

			foreach (var ip in ips)
			{
				try
				{
					var info = GetInfo(ip);
					if (info != null)
					{
						var prevInfo = await _dbContext.IpInfo.FirstOrDefaultAsync(x => x.Ip == info.Ip).ConfigureAwait(false);
						if (prevInfo == null)
						{
							prevInfo = new IpInfo { Ip = info.Ip };
							_dbContext.IpInfo.Add(prevInfo);
						}
						prevInfo.Company = info.Company;
						prevInfo.Country = info.Country;
						if (info.Subnet?.Length < 19)
							prevInfo.Subnet = info.Subnet;
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, ip);
				}
			}
			await _dbContext.SaveChangesAsync().ConfigureAwait(false);
			_logger.LogInformation("Completed update ip information");
		}

		private string TrimAll(string text)
		{
			return text.Trim('\t', '\n', '\r', ' ');
		}

		private IpInfo ParseResponse(WhoisResponse response, string ip)
		{
			_logger.LogInformation($"Trying parse response for ip {ip}");
			var lines = response.Raw.Split('\n').Select(x => TrimAll(x));
			var result = new IpInfo
			{
				Ip = ip,
				Company = response.OrganizationName
			};
			try
			{
				result.Subnet = response.AddressRange.ToCidrString();
			}
			catch
			{
				result.Subnet = response.AddressRange.ToString();
			}

			var countryLine = lines.FirstOrDefault(x => x.Contains("country:", StringComparison.CurrentCultureIgnoreCase));
			if (countryLine != null)
			{
				var country = TrimAll(countryLine.Split(':')[1]);
				result.Country = country;
			}
			return result;
		}

		private async Task<HashSet<string>> UnResolvedIps()
		{
			var attackedIps = await _dbContext.DnsAttacks
				.Select(x => x.Ip)
				.Distinct()
				.ToListAsync().ConfigureAwait(false);
			var suspectIps = await _dbContext.SuspectDomains
				.Select(x => x.Ip)
				.Distinct()
				.ToListAsync().ConfigureAwait(false);
			attackedIps.AddRange(suspectIps);
			attackedIps = attackedIps.Distinct().ToList();

			var resolvedIps = await _dbContext.IpInfo.Select(x => x.Ip).ToListAsync().ConfigureAwait(false);
			return attackedIps.Where(x => !resolvedIps.Contains(x)).ToHashSet();
		}
	}
}
