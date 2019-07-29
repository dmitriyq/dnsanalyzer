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
	public class AttackHub: Hub
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

		public async Task AttackList(DateTimeOffset? from, DateTimeOffset? to)
		{
			List<AttackGroups> attacks = null;
			if (!from.HasValue && !to.HasValue)
			{
				attacks = await _dbContext.AttackGroups
					.Include(x => x.Attacks)
					.ToListAsync();
			}
			else
			{
				var fromD = from?.Date ?? DateTimeOffset.UtcNow.Date;
				var toD = to?.Date.AddDays(1).AddMilliseconds(-1) ?? DateTimeOffset.UtcNow.Date.AddDays(1).AddMilliseconds(-1);

				await _dbContext.AttackGroups
					.Include(x => x.Attacks)
					.Where(x => x.DateBegin >= fromD && x.DateBegin <= toD)
					.ToListAsync();
			}
			var attackModels = attacks
				.OrderBy(x => x.Status)
				.ThenByDescending(x => x.DateBegin)
				.Select(x => _attackService.CastToViewModel(x))
				.ToList();
			await Clients.Caller.SendAsync("attacks", attackModels);
		}
	}
}
