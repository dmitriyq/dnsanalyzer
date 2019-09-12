using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grfc.Library.Notification.Models;

namespace Dns.Site.Services
{
	public interface IRedisService
	{
		Task PublishMessageAsync(string channel, byte[] message);
		Task PublishNotifyMessageAsync(NotificationBase notification);
	}
}
