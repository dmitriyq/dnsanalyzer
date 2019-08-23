using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Authorize(Policy = "DnsPolicy")]
	public class AuthorizedController : ControllerBase
	{
	}
}