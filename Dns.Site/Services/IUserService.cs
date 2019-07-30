using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dns.Site.AuthService;

namespace Dns.Site.Services
{
	public interface IUserService
	{

		Task CheckAuthAsync();

		Task LoginAsync(LoginModel model);
		Task LoginAsync(ClaimsPrincipal claimsPrincipal);

		Task LogoutAsync();

		Task<UserModel> GetCurrentUserAsync();
	}
}
