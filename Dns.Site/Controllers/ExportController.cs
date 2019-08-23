using System;
using System.Linq;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ExportController : AuthorizedController
	{
		private readonly ILogger<ExportController> _logger;
		private readonly ExcelService _excelService;
		private readonly DnsDbContext _dnsDb;
		private readonly IDistributedCache _cache;

		public ExportController(ILogger<ExportController> logger,
			DnsDbContext dnsDb, ExcelService excelService, IDistributedCache cache)
		{
			_logger = logger;
			_dnsDb = dnsDb;
			_excelService = excelService;
			_cache = cache;
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> ExportSuspect()
		{
			var file = await _cache.GetAsync("SuspectDomains");
			if (file == null)
			{
				var records = await _dnsDb.vSuspectDomains.ToListAsync();
				file = _excelService.ExportSuspectDomains(records);
				var expire = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };
				await _cache.SetAsync("SuspectDomains", file, expire);
			}
			return new JsonResult(new { success = true, msg = "SuspectDomains" });
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> ExportWhitelist()
		{
			var file = await _cache.GetAsync("WhiteList");
			if (file == null)
			{
				var records = await _dnsDb.WhiteDomains.ToListAsync();
				file = _excelService.ExportWhiteList(records);
				var expire = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };
				await _cache.SetAsync("WhiteList", file, expire);
			}
			return new JsonResult(new { success = true, msg = "WhiteList" });
		}

		public class ExportDnsParams
		{
			public DateTimeOffset From { get; set; }
			public DateTimeOffset To { get; set; }
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> ExportDns([FromBody] ExportDnsParams data)
		{
			var from = data.From.LocalDateTime;
			var to = data.To.LocalDateTime.Date.AddDays(1).AddMilliseconds(-1);
			var key = $"DNS_{Guid.NewGuid()}";

			var file = await _cache.GetAsync(key);
			if (file == null)
			{
				var records = await _dnsDb.AttackGroups
					.Include(x => x.Attacks)
					.Where(x => x.DateBegin >= from.Date && (!x.DateClose.HasValue || x.DateClose <= to)).ToListAsync();
				file = _excelService.ExportDnsAttack(records, from, to);
				var expire = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };
				await _cache.SetAsync(key, file, expire);
			}
			return new JsonResult(new { success = true, msg = key });
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> GetFile([FromQuery] string name)
		{
			await Task.CompletedTask;
			byte[] file = null;
			if (name == "SuspectDomains")
			{
				file = await _cache.GetAsync(name);
				if (file != null)
					return File(file, ExcelService.EXCEL_MIMETYPE, $"Suspect_Domains.xlsx");
			}
			else if (name.StartsWith("DNS_"))
			{
				file = await _cache.GetAsync(name);
				if (file != null)
					return File(file, ExcelService.EXCEL_MIMETYPE, $"DNS.xlsx");
			}
			else if (name == "WhiteList")
			{
				file = await _cache.GetAsync(name);
				if (file != null)
					return File(file, ExcelService.EXCEL_MIMETYPE, $"Белый список.xlsx");
			}
			return null;
		}

		[ResponseCache(Duration = 60)]
		[HttpGet("[action]")]
		public async Task<IActionResult> GetFirstDate()
		{
			var firstAttack = await _dnsDb.AttackHistories.MinAsync(x => x.Date);
			return new JsonResult(firstAttack);
		}
	}
}