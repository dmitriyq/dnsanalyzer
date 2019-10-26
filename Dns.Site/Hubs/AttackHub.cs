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
		private readonly ILogger<AttackHub> _logger;
		private readonly DnsDbContext _dbContext;
		private readonly AttackService _attackService;

		public AttackHub(ILogger<AttackHub> logger, DnsDbContext dnsDb, AttackService attackService)
		{
			_logger = logger;
			_dbContext = dnsDb;
			_attackService = attackService;
		}

		public async Task AttackList()
		{
			var attacks = await _dbContext.AttackGroups
					.Include(x => x.Attacks)
					.ToListAsync().ConfigureAwait(false);
			var attackModels = attacks
				.OrderBy(x => x.Status)
				.ThenByDescending(x => x.DateBegin)
				.Select(x => _attackService.CastToViewModel(x))
				.ToList();
			await Clients.Caller.SendAsync("attacks", attackModels).ConfigureAwait(false);
		}
	}
}