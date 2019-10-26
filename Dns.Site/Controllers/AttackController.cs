using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Contracts.Services;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.Site.Hubs;
using Dns.Site.Models;
using Dns.Site.Services;
using Grfc.Library.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AttackController : AuthorizedController
	{
		private readonly ILogger<AttackController> _logger;
		private readonly IHubContext<AttackHub> _hubContext;
		private readonly DnsDbContext _dnsDb;
		private readonly AttackService _attackService;
		private readonly INotifyService _notifyService;
		private readonly IRedisService _redisService;

		public AttackController(ILogger<AttackController> logger,
			IHubContext<AttackHub> hubContext,
			DnsDbContext dnsDb,
			AttackService attackService,
			INotifyService notifyService,
			IRedisService redis)
		{
			_logger = logger;
			_hubContext = hubContext;
			_dnsDb = dnsDb;
			_attackService = attackService;
			_notifyService = notifyService;
			_redisService = redis;
		}

		[HttpGet]
		public IActionResult Attacks()
		{
			var attackModels = _dnsDb.AttackGroups
				.Include(x => x.Attacks)
				.AsEnumerable()
				.OrderBy(x => x.Status)
				.ThenByDescending(x => x.DateBegin)
				.Select(x => _attackService.CastToViewModel(x))
				.ToList();
			return new JsonResult(attackModels);
		}

		[HttpGet("[action]")]
		public IActionResult Info([FromQuery]int id)
		{
			try
			{
				var mdl = _attackService.GetViewModel(id);
				return new JsonResult(mdl);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "");
			}
			return new JsonResult(null);
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Edit([FromBody]AttackEditParams data)
		{
			try
			{
				var attack = await _dnsDb.AttackGroups
					.FirstOrDefaultAsync(x => x.Id == data.Id).ConfigureAwait(false);
				if (attack != null)
				{
					var prevStatus = attack.StatusEnum;
					attack.Status = data.Status;
					await _attackService.AddHistory(attack, prevStatus).ConfigureAwait(false);
					if (!data.Comment.IsBlank())
						await _attackService.AddNote(attack, data.Comment).ConfigureAwait(false);
					await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
					await _hubContext.Clients.All.SendAsync("UpdateAttack", _attackService.CastToViewModel(attack)).ConfigureAwait(false);

					var attackMessage = _notifyService.BuildAttackMessage(string.Empty, attack.Id);
					await _redisService.PublishNotifyMessageAsync(attackMessage).ConfigureAwait(false);

					return new JsonResult("Ok");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "");
			}
			return new JsonResult(null);
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Note([FromBody]AttackNoteParams data)
		{
			try
			{
				var attack = await _dnsDb.AttackGroups
					.FirstOrDefaultAsync(x => x.Id == data.Id).ConfigureAwait(false);
				if (attack != null)
				{
					if (!data.Comment.IsBlank())
					{
						await _attackService.AddNote(attack, data.Comment).ConfigureAwait(false);
						await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
						await _hubContext.Clients.All.SendAsync("UpdateAttack", _attackService.CastToViewModel(attack)).ConfigureAwait(false);
						return new JsonResult("Ok");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "");
			}
			return new JsonResult(null);
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> EditMany([FromBody]AttackEditManyParams data)
		{
			try
			{
				var attacks = _dnsDb.AttackGroups
					.Include(x => x.Attacks)
					.Where(x => data.Ids.Contains(x.Id))
					.ToList();
				foreach (var attack in attacks)
				{
					var prevStatus = attack.StatusEnum;
					attack.Status = data.Status;
					await _attackService.AddHistory(attack, prevStatus).ConfigureAwait(false);
					if (!data.Comment.IsBlank())
						await _attackService.AddNote(attack, data.Comment).ConfigureAwait(false);
					await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
					await _hubContext.Clients.All.SendAsync("UpdateAttack", _attackService.CastToViewModel(attack)).ConfigureAwait(false);
				}

				var attackMessage = _notifyService.BuildAttackMessage(string.Empty, attacks.Select(x => x.Id).ToArray());
				await _redisService.PublishNotifyMessageAsync(attackMessage).ConfigureAwait(false);

				return new JsonResult("Ok");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "");
			}
			return new JsonResult(null);
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> Stats()
		{
			var dateBegin = DateTimeOffset.UtcNow.TimeOfDay < TimeSpan.FromMinutes((5 * 60) + 30) ?
				DateTimeOffset.UtcNow.AddDays(-1).Date.AddMinutes((5 * 60) + 30) :
				DateTimeOffset.UtcNow.Date.AddMinutes((5 * 60) + 30);

			var groups = _dnsDb.AttackGroups;
			var groupDailyNew = await groups
				.CountAsync(x => x.GroupHistories.Any(z => z.Date > dateBegin && z.PrevStatus == (int)AttackGroupStatusEnum.None)).ConfigureAwait(false);
			var groupTotal = await groups.CountAsync().ConfigureAwait(false);
			var groupThreatCount = await groups.CountAsync(x => x.Status == (int)AttackGroupStatusEnum.Threat).ConfigureAwait(false);
			var groupDymanicCount = await groups.CountAsync(x => x.Status == (int)AttackGroupStatusEnum.Dynamic).ConfigureAwait(false);
			var groupAttackCount = await groups.CountAsync(x => x.Status == (int)AttackGroupStatusEnum.Attack).ConfigureAwait(false);
			var groupCompletedCount = await groups.CountAsync(x => x.Status == (int)AttackGroupStatusEnum.Complete).ConfigureAwait(false);

			var attacks = _dnsDb.DnsAttacks;
			var attackDailyNew = await attacks
				.CountAsync(x => x.Histories.Any(z => z.Date > dateBegin && z.PrevStatus == (int)AttackStatusEnum.None)).ConfigureAwait(false);
			var attackTotal = await attacks.CountAsync().ConfigureAwait(false);
			var attackIntersectionCount = await attacks.CountAsync(x => x.Status == (int)AttackStatusEnum.Intersection || x.Status == (int)AttackStatusEnum.Closing).ConfigureAwait(false);
			var attackCompletedCount = await attacks.CountAsync(x => x.Status == (int)AttackStatusEnum.Completed).ConfigureAwait(false);

			return new JsonResult(new
			{
				groups = new List<AttackStatsJson> {
					new AttackStatsJson{Name = "Новые за сутки", Count = groupDailyNew},
					new AttackStatsJson{Name = "Общее число атак", Count = groupTotal},
					new AttackStatsJson{Name = "Угроза", Count = groupThreatCount},
					new AttackStatsJson{Name = "Динамические IP", Count = groupDymanicCount},
					new AttackStatsJson{Name = "Атака", Count = groupAttackCount},
					new AttackStatsJson{Name = "Прекращено", Count = groupCompletedCount},
				},
				attacks = new List<AttackStatsJson> {
					new AttackStatsJson{Name = "Новые за сутки", Count = attackDailyNew},
					new AttackStatsJson{Name = "Общее число атак", Count = attackTotal},
					new AttackStatsJson{Name = "Пересечений", Count = attackIntersectionCount},
					new AttackStatsJson{Name = "Пересечений снято", Count = attackCompletedCount},
				},
			});
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> Calendar([FromQuery]int year, int month)
		{
			var startMonth = new DateTimeOffset(new DateTime(year, month, 1), TimeSpan.Zero).Date;
			var endMonth = startMonth.AddMonths(1).AddMilliseconds(-1);
			startMonth = startMonth.AddDays(-1);
			var stats = await _dnsDb.StatisticHistories.Where(x => x.Date >= startMonth && x.Date <= endMonth).ToListAsync().ConfigureAwait(false);
			stats.ForEach(x => x.Date = x.Date.AddDays(-1));
			return new JsonResult(stats);
		}

		[ResponseCache(Duration = 30)]
		[HttpGet("[action]")]
		public async Task<IActionResult> Statuses()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			var statuses = EnumExtensions.GetEnumsWithDisplayValues<AttackStatusEnum>()
				.Select(x => new { text = x.Value, value = x.Key });
			return new JsonResult(statuses.ToArray());
		}

		[ResponseCache(Duration = 30)]
		[HttpGet("[action]")]
		public async Task<IActionResult> GroupStatuses()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			var statuses = EnumExtensions.GetEnumsWithDisplayValues<AttackGroupStatusEnum>()
				.Select(x => new { text = x.Value, value = x.Key });
			return new JsonResult(statuses.ToArray());
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> Suspects()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			var model = _dnsDb.vSuspectDomains
				.ToList()
				.GroupBy(x => x.Domain)
				.Select(x => new SuspectDomainViewModel
				{
					Domain = x.Key,
					Ips = x.Select(z => new SuspectDomainViewModel.IpInfo
					{
						Company = z.Company,
						Country = z.Country,
						Ip = z.Ip,
						Subnet = z.Subnet
					}).OrderBy(z => z.Ip).ToList()
				}).OrderByDescending(x => x.Ips.Count).ThenBy(x => x.Domain).ToList();
			return new JsonResult(new { data = model });
		}
	}
}