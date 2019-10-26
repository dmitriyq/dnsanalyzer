using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Models;
using Dns.Site.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Site.Hubs
{
	public class AttackHub : Hub
	{
		private readonly DnsDbContext _dbContext;
		private readonly AttackService _attackService;

		public AttackHub(DnsDbContext dnsDb, AttackService attackService)
		{
			_dbContext = dnsDb;
			_attackService = attackService;
		}

		public async Task AttackList()
		{
			var attackModels = _dbContext.AttackGroups
				.Include(x => x.Attacks)
				.AsEnumerable()
				.OrderBy(x => x.Status)
				.ThenByDescending(x => x.DateBegin)
				.Select(x => _attackService.CastToViewModel(x))
				.ToList();
			await Clients.Caller.SendAsync("Attacks", attackModels).ConfigureAwait(false);
		}
	}
}