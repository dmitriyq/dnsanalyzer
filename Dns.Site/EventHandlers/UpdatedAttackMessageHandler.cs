using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.DAL;
using Dns.Site.Hubs;
using Dns.Site.Services;
using Grfc.Library.EventBus.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dns.Site.EventHandlers
{
	public class UpdatedAttackMessageHandler : IAmqpMessageHandler<UpdatedAttackMessage>
	{
		private readonly IHubContext<AttackHub> _hubContext;
		private readonly AttackService _attackService;
		private readonly DnsDbContext _dnsDb;

		public UpdatedAttackMessageHandler(IHubContext<AttackHub> hubContext, AttackService attackService, DnsDbContext dnsDb)
		{
			_hubContext = hubContext;
			_attackService = attackService;
			_dnsDb = dnsDb;
		}

		public async Task Handle(UpdatedAttackMessage message)
		{
			if (message.AttackIds.Count > 0)
			{
				var groups = await _dnsDb.DnsAttacks.Where(x => message.AttackIds.Contains(x.Id))
					.Include(x => x.AttackGroup)
					.Select(x => x.AttackGroup)
					.Distinct()
					.ToListAsync()
					.ConfigureAwait(true);

				foreach (var attack in groups)
				{
					await _hubContext.Clients.All.SendAsync("UpdateAttack", _attackService.CastToViewModel(attack)).ConfigureAwait(true);
				}
			}
			if (message.GroupIds.Count > 0)
			{
				var groups = await _dnsDb.AttackGroups.Where(x => message.AttackIds.Contains(x.Id))
					.Distinct()
					.ToListAsync()
					.ConfigureAwait(true);
				foreach (var attack in groups)
				{
					await _hubContext.Clients.All.SendAsync("UpdateAttack", _attackService.CastToViewModel(attack)).ConfigureAwait(true);
				}
			}
		}
	}
}
