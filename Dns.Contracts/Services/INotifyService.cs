using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grfc.Library.Notification.Interfaces;
using Grfc.Library.Notification.Messages;

namespace Dns.Contracts.Services
{
	public interface INotifyService: INotifyClientService
	{
		NotificationMessage BuildAttackMessage(string user, params int[] attackIds);
		NotificationMessage BuildGroupMessage(string user, params int[] groupIds);
	}
}
