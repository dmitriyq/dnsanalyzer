using System;
using System.Linq;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Dns.Site.Models;
using Microsoft.EntityFrameworkCore;

namespace Dns.Site.Services
{
	public class AttackService
	{
		private DnsDbContext _dnsDb;

		public AttackService(DnsDbContext dnsDbContext)
		{
			_dnsDb = dnsDbContext;
		}

		public AttackGroupViewModel CastToViewModel(AttackGroups attackGroup)
		{
			return new AttackGroupViewModel
			{
				Summary = attackGroup.Attacks.GroupBy(x => x.Status).Select(x => new AttackGroupViewModel.AttacksSummaryViewModel
				{
					Count = x.Count(),
					Status = x.Key
				}).OrderBy(x => x.Status).ToList(),
				BlackDomain = attackGroup.Attacks.FirstOrDefault()?.BlackDomain,
				Status = attackGroup.Status,
				Id = attackGroup.Id,
				WhiteDomain = attackGroup.Attacks.FirstOrDefault()?.WhiteDomain
			};
		}

		public async Task AddNote(AttackGroups attackGroup, string text, bool commit = false)
		{
			attackGroup.Notes.Add(new AttackNotes
			{
				AttackGroup = attackGroup,
				AttackGroupId = attackGroup.Id,
				Date = DateTimeOffset.UtcNow,
				Text = text.Trim()
			});
			if (commit)
				await _dnsDb.SaveChangesAsync();
			else await Task.FromResult(0);
		}

		public async Task AddHistory(AttackGroups attackGroup, AttackGroupStatusEnum prevStatusEnum, bool commit = false)
		{
			attackGroup.GroupHistories.Add(new AttackGroupHistories
			{
				AttackGroupId = attackGroup.Id,
				Date = DateTimeOffset.UtcNow,
				CurrentStatus = attackGroup.Status,
				PrevStatus = (int)prevStatusEnum,
			});
			if (commit)
				await _dnsDb.SaveChangesAsync();
			else await Task.FromResult(0);
		}

		public AttackInfoViewModel GetViewModel(int attackGroupId)
		{
			var attack = _dnsDb.AttackGroups
				.Include(x => x.Attacks).ThenInclude(x => x.Histories)
				.Include(x => x.GroupHistories)
				.Include(x => x.Notes)
				.FirstOrDefault(x => x.Id == attackGroupId);
			if (attack != null)
			{
				var blackDomainName = attack.Attacks.FirstOrDefault().BlackDomain;
				var whiteDomainName = attack.Attacks.FirstOrDefault().WhiteDomain;
				var blackDomainInfo = _dnsDb.DomainInfo
					.Include(x => x.DomainNS)
					.ThenInclude(x => x.NameServer)
					.FirstOrDefault(x => x.Domain == blackDomainName);
				var whiteDomainInfo = _dnsDb.DomainInfo
					.Include(x => x.DomainNS)
					.ThenInclude(x => x.NameServer)
					.FirstOrDefault(x => x.Domain == whiteDomainName);
				var attackIps = attack.Attacks.Select(x => x.Ip).ToList();
				var ipInfos = _dnsDb.IpInfo.Where(x => attackIps.Contains(x.Ip)).ToList();
				return new AttackInfoViewModel
				{
					Attacks = attack.Attacks.OrderBy(x => x.Ip).Select(x => new AttackInfoViewModel.AttackIpViewModel
					{
						Id = x.Id,
						Ip = x.Ip,
						IpBlocked = x.IpBlocked || (!string.IsNullOrWhiteSpace(x.SubnetBlocked)),
						Status = x.Status,
						SubnetBlocked = x.SubnetBlocked,
						Histories = x.Histories.OrderBy(z => z.Date).Select(z => new AttackInfoViewModel.History
						{
							Id = z.Id,
							Create = z.Date,
							CurrentStatus = z.CurrentStatus,
							PrevStatus = z.PrevStatus
						}).ToList(),
						IpInfo = ipInfos.Where(z => z.Ip == x.Ip).Select(z => new AttackInfoViewModel.IpViewModel
						{
							Company = z.Company,
							Country = z.Country,
							Ip = z.Ip,
							Subnet = z.Subnet
						}).FirstOrDefault()
					}).ToList(),
					Begin = attack.DateBegin,
					BlackDomain = blackDomainName,
					BlackDomainInfo = blackDomainInfo != null ? new AttackInfoViewModel.DomainViewModel
					{
						Company = blackDomainInfo.Company,
						DateCreate = blackDomainInfo.DateCreate,
						DateUntil = blackDomainInfo.DateUntil,
						Domain = blackDomainInfo.Domain,
						NameServers = blackDomainInfo.DomainNS.Select(z => z.NameServer.NameServer).OrderBy(x => x).ToList(),
						Registrant = blackDomainInfo.Registrant,
					} : null,
					Close = attack.DateClose,
					Histories = attack.GroupHistories.OrderBy(x => x.Date).Select(z => new AttackInfoViewModel.History
					{
						Create = z.Date,
						CurrentStatus = z.CurrentStatus,
						Id = z.Id,
						PrevStatus = z.PrevStatus
					}).ToList(),
					Id = attack.Id,
					Notes = attack.Notes.OrderBy(x => x.Date).Select(z => new AttackInfoViewModel.NoteViewModel
					{
						AttackId = z.AttackGroupId,
						Id = z.Id,
						Create = z.Date,
						Text = z.Text
					}).ToList(),
					Status = attack.Status,
					WhiteDomain = whiteDomainName,
					WhiteDomainInfo = whiteDomainInfo != null ? new AttackInfoViewModel.DomainViewModel
					{
						Company = whiteDomainInfo.Company,
						DateCreate = whiteDomainInfo.DateCreate,
						DateUntil = whiteDomainInfo.DateUntil,
						Domain = whiteDomainInfo.Domain,
						NameServers = whiteDomainInfo.DomainNS.Select(z => z.NameServer.NameServer).OrderBy(x => x).ToList(),
						Registrant = whiteDomainInfo.Registrant,
					} : null,
				};
			}
			return null;
		}
	}
}