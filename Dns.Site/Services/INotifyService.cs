using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grfc.Library.Notification.Models;

namespace Dns.Site.Services
{
	public interface INotifyService
	{
		Task<NotificationBase> BuildAttackMessage(string user, params int[] attackIds);
		Task<NotificationBase> BuildGroupMessage(string user, params int[] attackIds);
	}
}
