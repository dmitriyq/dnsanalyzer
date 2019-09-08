using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Site.Services;
using Dns.Site.VigruzkiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReestrController : AuthorizedController
	{
		private readonly ILogger<ReestrController> _logger;
		private readonly IVigruzkiService _vigruzkiService;

		public ReestrController(ILogger<ReestrController> logger, IVigruzkiService vigruzkiService)
		{
			_logger = logger;
			_vigruzkiService = vigruzkiService;
		}

		[HttpGet("[action]")]
		public async Task<IActionResult> IsBlocked([FromQuery]string search)
		{
			var contains = await _vigruzkiService.SearchContainsAsync(search).ConfigureAwait(false);
			return new JsonResult(contains.Count > 0);
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Query([FromBody]QueryModel model)
		{
			IEnumerable<VigruzkiRecordModel> result;
			if (!model.IsContainsSearch)
			{
				result = await _vigruzkiService.SearchEqualAsync(model.Search).ConfigureAwait(false);
			}
			else
			{
				result = await _vigruzkiService.SearchContainsAsync(model.Search).ConfigureAwait(false);
			}

			return new JsonResult(result);
		}

		public class QueryModel
		{
			public string Search { get; set; }
			public bool IsContainsSearch { get; set; }
		}
	}
}