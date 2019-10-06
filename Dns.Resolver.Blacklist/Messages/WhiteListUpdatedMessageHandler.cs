using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dns.Contracts.Messages;
using Dns.Contracts.Protobuf;
using Dns.Resolver.Blacklist.Services.Interfaces;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;

namespace Dns.Resolver.Blacklist.Messages
{
	public class WhiteListUpdatedMessageHandler : IAmqpMessageHandler<WhiteListUpdatedMessage>
	{
		private readonly ICacheService _cacheService;

		public WhiteListUpdatedMessageHandler(ICacheService cacheService)
		{
			_cacheService = cacheService;
		}

		public Task Handle(WhiteListUpdatedMessage message) =>
			_cacheService.UpdateWhiteListAsync(message.ProtoSerializedData.ProtoDeserialize<ResolvedDomain[]>());
	}
}
