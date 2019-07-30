using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Site.AuthService;
using Dns.Site.Services;
using Grfc.Library.Auth.Extensions;
using Grfc.Library.Common.Enums;
using Grfc.Library.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dns.Site.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : AuthorizedController
	{
		private readonly ILogger<AccountController> _logger;
		private readonly IUserService _userService;
		private readonly INotifyApiService _notifyService;

		public AccountController(ILogger<AccountController> logger, IUserService userService, INotifyApiService notifyService)
		{
			_logger = logger;
			_userService = userService;
			_notifyService = notifyService;
		}

		/// <summary>
		/// Получение информации о текущем пользователе
		/// </summary>
		/// <returns></returns>
		[HttpGet("[action]")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(UserModel) ,200)]
		[ProducesResponseType(401)]
		public async Task<JsonResult> GetUserInfo()
		{
			await _userService.LoginAsync(User);
			var user = await _userService.GetCurrentUserAsync();

			return new JsonResult(user);
		}

		/// <summary>
		/// Проверка авторизации
		/// </summary>
		/// <returns></returns>
		[HttpGet("[action]")]
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(401)]
		public JsonResult CheckAuth()
			=> new JsonResult(new { ok = true });

		/// <summary>
		/// Список адресов для уведомлений текущего пользователя
		/// </summary>
		/// <returns></returns>
		[HttpGet("[action]")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(IEnumerable<string>), 200)]
		[ProducesResponseType(401)]
		public async Task<JsonResult> UserNotifications()
		{
			var userCred = User.CurrentCredentials();
			await _notifyService.AuthorizeAsync(userCred.login, userCred.pass);
			var notifies = await _notifyService.UserNotificationsAsync(userCred.login);

			return new JsonResult(notifies.Select(x => x.Value).ToArray());

		}

		/// <summary>
		/// Замена всех адресов уведомлений на новые
		/// </summary>
		/// <param name="notifies">новые адреса уведомлений</param>
		/// <returns></returns>
		[HttpPost("[action]")]
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(401)]
		public async Task<JsonResult> UserNotifications([FromBody]string[] notifies)
		{

			var userCred = User.CurrentCredentials();
			await _notifyService.AuthorizeAsync(userCred.login, userCred.pass);
			await _notifyService.UpdateNotificationsAsync(userCred.login, notifies);

			return new JsonResult(new { ok = true });
		}
	}
}
