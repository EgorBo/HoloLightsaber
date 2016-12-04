using System.IO;
using ProtoBuf;

namespace Shared
{
	/// <summary>
	/// Protobuf based impl
	/// uses WithLengthPrefix
	/// </summary>
	public class ProtobufNetworkSerializer : INetworkSerializer
	{
		public byte[] Serialize(BaseDto dto)
		{
			using (var ms = new MemoryStream())
			{
				Serializer.SerializeWithLengthPrefix(ms, dto, PrefixStyle.Base128, fieldNumber: 1);
				return ms.ToArray();
			}
		}

		public T Deserialize<T>(byte[] data) where T : BaseDto
		{
			using (var ms = new MemoryStream(data))
			{
				return Serializer.DeserializeWithLengthPrefix<T>(ms, PrefixStyle.Base128, fieldNumber: 1);
			}
		}
	}
}
