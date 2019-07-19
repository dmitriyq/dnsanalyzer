using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dns.Library.Helpers
{
	public static class SerializeExtensions
	{
		public static byte[] ProtoSerialize<T>(this T item)
		{
			using var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, item);
			return ms.ToArray();
		}

		public static T ProtoDeserialize<T>(this byte[] data)
		{
			using var ms = new MemoryStream(data);
			return ProtoBuf.Serializer.Deserialize<T>(ms);
		}
	}
}
