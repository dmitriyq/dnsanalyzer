using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grfc.Library.Common.Extensions;
using Grfc.Library.Notification.Models;
using StackExchange.Redis;

namespace Dns.Site.Services
{
	public class RedisService : IRedisService
	{
		private readonly IDatabase _redisDb;
		private readonly ISubscriber _redisSubscriber;
		private readonly string _notifyMessageChannel;

		public RedisService(ConnectionMultiplexer redis, string notifyMessageChannel)
		{
			_redisDb = redis.GetDatabase();
			_redisSubscriber = redis.GetSubscriber();
			_notifyMessageChannel = notifyMessageChannel;
		}

		public Task PublishMessageAsync(string channel, byte[] message)
		{
			return _redisSubscriber.PublishAsync(channel, message);
		}

		public Task PublishNotifyMessageAsync(NotificationBase notification)
		{
			return _redisSubscriber.PublishAsync(_notifyMessageChannel, notification.ProtoSerialize());
		}
	}
}
