using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Dns.DAL;
using Dns.DAL.Models;
using Dns.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class WhiteListController : AuthorizedController
	{
		private readonly ILogger<WhiteListController> _logger;
		private readonly DnsDbContext _dnsDb;

		public WhiteListController(ILogger<WhiteListController> logger, DnsDbContext dnsDb)
		{
			_logger = logger;
			_dnsDb = dnsDb;
		}

		[HttpGet]
		public async Task<IActionResult> GetWhiteList()
		{
			var entities = _dnsDb.WhiteDomains;
			var domains = await CastToModel(entities).ToListAsync().ConfigureAwait(false);
			return new JsonResult(domains);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetWhiteDomain(int id)
		{
			var domain = await _dnsDb.WhiteDomains.FindAsync(id).ConfigureAwait(false);
			if (domain == null)
				return NotFound(ErrorMessage.NotFound);
			var model = CastToModel(domain);
			return new JsonResult(model);
		}

		[HttpPost]
		public async Task<IActionResult> CreateDomain(WhiteDomainViewModel data)
		{
			if (data == null && string.IsNullOrWhiteSpace(data.Domain))
				return BadRequest(ErrorMessage.InvalidId);
			data.Domain = data.Domain.Trim();
			if (await _dnsDb.WhiteDomains.AnyAsync(x => x.Domain == data.Domain).ConfigureAwait(false))
				return BadRequest(ErrorMessage.AlreadyInList);
			var entity = _dnsDb.WhiteDomains.Add(new WhiteDomains { Domain = data.Domain, DateAdded = DateTime.Now });
			await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
			var model = CastToModel(entity.Entity);
			return CreatedAtAction(nameof(GetWhiteDomain), new { id = model.Id }, model);
		}

		[HttpPost("batch")]
		public async Task<IActionResult> CreateDomains([FromBody] IEnumerable<string> domains)
		{
			List<WhiteDomains> newDomains = new List<WhiteDomains>();
			foreach (var item in domains)
			{
				if (!(await _dnsDb.WhiteDomains.AnyAsync(x => x.Domain == item).ConfigureAwait(false)))
					newDomains.Add(new WhiteDomains { Domain = item, DateAdded = DateTime.Now });
			}
			_dnsDb.WhiteDomains.AddRange(newDomains);
			await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
			var models = newDomains.Select(x => CastToModel(x)).ToList();
			return new JsonResult(models);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateDomain(int id, WhiteDomainViewModel item)
		{
			if (id != item.Id)
				return BadRequest(ErrorMessage.InvalidId);

			var entity = await _dnsDb.WhiteDomains.FindAsync(id).ConfigureAwait(false);
			if (entity == null)
				return NotFound(ErrorMessage.NotFound);
			if (await _dnsDb.WhiteDomains.AnyAsync(x => x.Domain == item.Domain).ConfigureAwait(false))
				return BadRequest(ErrorMessage.AlreadyInList);
			entity.Domain = item.Domain;
			await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteDomain(int id)
		{
			var entity = await _dnsDb.WhiteDomains.FindAsync(id).ConfigureAwait(false);
			if (entity == null)
				return NotFound(ErrorMessage.NotFound);
			_dnsDb.WhiteDomains.Remove(entity);
			await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
			return NoContent();
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteDomains([FromBody] IEnumerable<int> ids)
		{
			List<WhiteDomains> deleteDomains = new List<WhiteDomains>();
			foreach (var item in ids)
			{
				var domain = await _dnsDb.WhiteDomains.FindAsync(item).ConfigureAwait(false);
				if (domain != null)
					deleteDomains.Add(domain);
			}
			_dnsDb.WhiteDomains.RemoveRange(deleteDomains);
			await _dnsDb.SaveChangesAsync().ConfigureAwait(false);
			return NoContent();
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadDomains()
		{
			HashSet<string> domains = null;
			var file = Request.Form.Files[0];
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			using (var ms = new MemoryStream())
			{
				file.CopyTo(ms);
				if (file.FileName.EndsWith(".xlsx"))
				{
					using var xlReader = new XLWorkbook(ms, XLEventTracking.Disabled);
					var cells = xlReader.Worksheets.First().CellsUsed().Select(x => x.GetString() ?? "")
						.Where(x => !string.IsNullOrWhiteSpace(x) && Uri.CheckHostName(x) == UriHostNameType.Dns)
						.Select(x => x.Trim());
					domains = new HashSet<string>(cells);
				}
				else if (file.FileName.EndsWith(".csv") || file.FileName.EndsWith(".txt"))
				{
					var lines = Encoding.GetEncoding("windows-1251").GetString(ms.ToArray()).Split(new char[] { '\r', '\n', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
					domains = new HashSet<string>(lines.Where(x => !string.IsNullOrWhiteSpace(x) && Uri.CheckHostName(x) == UriHostNameType.Dns).Select(x => x.Trim()));
				}
			}
			var validDomains = new List<string>();
			var mapping = new System.Globalization.IdnMapping();
			foreach (var domain in domains.Where(x => x.Contains('.')))
			{
				try
				{
					var entries = await System.Net.Dns.GetHostEntryAsync(mapping.GetAscii(domain)).ConfigureAwait(false);
					if (entries != null && (!string.IsNullOrWhiteSpace(entries.HostName)))
						validDomains.Add(domain);
				}
				catch { }
			}
			var invalidDomains = domains.Except(validDomains).ToList();
			await Task.FromResult(0).ConfigureAwait(false);
			return new JsonResult(new { success = validDomains, error = invalidDomains });
		}

		private WhiteDomainViewModel CastToModel(WhiteDomains item)
			=> new WhiteDomainViewModel { DateAdded = item.DateAdded, Domain = item.Domain, Id = item.Id };

		private IQueryable<WhiteDomainViewModel> CastToModel(IQueryable<WhiteDomains> items)
			=> items.Select(x => CastToModel(x));

		public class ErrorMessage
		{
			public string Msg { get; set; }

			public ErrorMessage()
			{
			}

			public ErrorMessage(string msg)
			{
				this.Msg = msg;
			}

			public static ErrorMessage InvalidId => new ErrorMessage("Не корректный домен");
			public static ErrorMessage NotFound => new ErrorMessage("Указанный домен не найден");
			public static ErrorMessage AlreadyInList => new ErrorMessage("Домен уже присутствует в списке");
		}
	}
}