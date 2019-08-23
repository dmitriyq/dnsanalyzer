using System.Collections.Generic;
using System.Threading.Tasks;
using Dns.Site.NotificationService;

namespace Dns.Site.Services
{
	public interface INotifyApiService
	{
		Task AuthorizeAsync(string login, string pass);

		Task<IEnumerable<NotifyModel>> UserNotificationsAsync(string login);

		Task UpdateNotificationsAsync(string login, string[] notifies);
	}
}