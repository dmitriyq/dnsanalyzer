using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class AttackBatchCreatedMessage: AmqpMessage
	{
		public AttackFoundMessage[] AttackMessages { get; set; }

		public AttackBatchCreatedMessage(AttackFoundMessage[] attackMessages)
		{
			AttackMessages = attackMessages.ToArray();
		}
	}
}
