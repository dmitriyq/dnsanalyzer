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
				var groupIds = _dnsDb.DnsAttacks.Where(x => message.AttackIds.Contains(x.Id))
					.Select(x => x.AttackGroupId).Distinct().ToList();
				var groups = _dnsDb.AttackGroups
					.Include(x => x.Attacks)
					.Where(x => groupIds.Contains(x.Id))
					.Distinct()
					.ToList();
				foreach (var attack in groups)
				{
					var model = _attackService.CastToViewModel(attack);
					await _hubContext.Clients.All.SendAsync("UpdateAttack", model).ConfigureAwait(false);
				}
			}
			if (message.GroupIds.Count > 0)
			{
				var groups = _dnsDb.AttackGroups
					.Include(x => x.Attacks)
					.Where(x => message.GroupIds.Contains(x.Id))
					.Distinct()
					.ToList();
				foreach (var attack in groups)
				{
					var model = _attackService.CastToViewModel(attack);
					await _hubContext.Clients.All.SendAsync("UpdateAttack", model).ConfigureAwait(true);
				}
			}
		}
	}
}
