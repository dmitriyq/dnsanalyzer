using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grfc.Library.Notification.Models;

namespace Dns.Contracts.Services
{
	public interface INotifyService
	{
		Task<NotificationBase> BuildAttackMessage(string user, params int[] attackIds);
		Task<NotificationBase> BuildGroupMessage(string user, params int[] attackIds);
	}
}
