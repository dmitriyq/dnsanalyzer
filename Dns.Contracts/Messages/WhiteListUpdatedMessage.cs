using System;
using System.Collections.Generic;
using System.Text;
using Grfc.Library.EventBus.Abstractions.Messages;

namespace Dns.Contracts.Messages
{
	public class WhiteListUpdatedMessage: AmqpMessage
	{
		public byte[] ProtoSerializedData { get; set; }

		public WhiteListUpdatedMessage(byte[] data)
		{
			ProtoSerializedData = data;
		}
	}
}
