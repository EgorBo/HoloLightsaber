namespace Shared
{
	public interface INetworkSerializer
	{
		byte[] Serialize(BaseDto dto);
		T Deserialize<T>(byte[] currentProperty) where T : BaseDto;
	}
}