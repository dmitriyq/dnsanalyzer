using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dns.Library;
using Dns.Site.AuthService;
using Dns.Site.Services;
using Grfc.Library.Common.Extensions;

namespace Dns.Site.NotificationService
{
	public partial class Client : INotifyApiService
	{
		public Task AuthorizeAsync(string login, string pass)
		{
			var authServer = EnvironmentExtensions.GetVariable(EnvVars.AUTH_SERVER_URL);
			var loginUrl = authServer.TrimEnd('/') + "/api/Auth/Login";
			var loginModel = new LoginModel
			{
				Login = login,
				Password = pass
			};
			var body = new StringContent(loginModel.ToJson(), Encoding.Default, MediaTypeHeaderValue.Parse("application/json").MediaType);
			return _httpClient.PostAsync(new Uri(loginUrl, UriKind.Absolute), body);
		}

		public async Task UpdateNotificationsAsync(string login, string[] notifies)
		{
			var trimmedNotifies = notifies.Select(x => x.Trim()).ToList();

			var userNotifies = await UserNotificationsAsync(login);

			var toDelete = userNotifies.Where(x => !trimmedNotifies.Contains(x.Value));
			var toAdd = trimmedNotifies.Where(x => !userNotifies.Any(z => z.Value == x));

			foreach (var delNotify in toDelete)
			{
				await Notification5Async(delNotify.Id);
			}
			foreach (var addNotify in toAdd)
			{
				var phoneRegex = new Regex(@"^((\+7)|(8))\d{10}$", RegexOptions.Compiled);
				var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

				var model = new NotifyModel()
				{
					SiteType = NotifyModelSiteType.Dns,
					UserName = login,
					Value = addNotify
				};
				if (phoneRegex.IsMatch(addNotify))
					model.NotifyType = NotifyModelNotifyType.Sms;
				else if (emailRegex.IsMatch(addNotify))
					model.NotifyType = NotifyModelNotifyType.Email;
				else continue;

				await NotificationAsync(model);
			}
		}

		public async Task<IEnumerable<NotifyModel>> UserNotificationsAsync(string login)
		{
			var notify = await Notification2Async(login);
			return notify.AsEnumerable();
		}
	}
}