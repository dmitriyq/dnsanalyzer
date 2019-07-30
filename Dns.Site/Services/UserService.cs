using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dns.Site.Services;
using Grfc.Library.Auth.Extensions;

namespace Dns.Site.AuthService
{
	public partial class Client : IUserService
	{
		public Task<UserModel> GetCurrentUserAsync()
		{
			return SelfAsync();
		}

		public Task LoginAsync(ClaimsPrincipal claimsPrincipal)
		{
			var userCred = claimsPrincipal.CurrentCredentials();
			var loginModel = new LoginModel
			{
				Login = userCred.login,
				Password = userCred.pass
			};
			return LoginAsync(loginModel);
		}
	}
}
