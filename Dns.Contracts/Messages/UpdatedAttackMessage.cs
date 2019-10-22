using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class UpdatedAttackMessage: AmqpMessage
	{
		public List<int> AttackIds { get; set; } = new List<int>();
		public List<int> GroupIds { get; set; } = new List<int>();

		public UpdatedAttackMessage(List<int> attackIds, List<int> groupIds)
		{
			AttackIds = attackIds.ToList();
			GroupIds = groupIds.ToList();
		}
	}
}
