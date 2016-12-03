using System;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;

namespace Lightsaber
{
	public class HoloLensConnection
	{
		const int Port = 5206;
		UdpSocketReceiver listener;

		string remoteAddress;
		string remotePort;

		public INetworkSerializer Serializer { get; private set; }

		public async Task WaitForCompanion()
		{
			Serializer = new ProtobufNetworkSerializer();
			var tcs = new TaskCompletionSource<bool>();
			listener = new UdpSocketReceiver();
			listener.MessageReceived += (s, e) =>
				{
					remoteAddress = e.RemoteAddress;
					remotePort = e.RemotePort;

					var dto = Serializer.Deserialize<BaseDto>(e.ByteData);
					tcs.TrySetResult(true);
				};
			await listener.StartListeningAsync(Port);
			await tcs.Task;
		}
		
		public async void Send(BaseDto dto)
		{
			try
			{
				await listener.SendToAsync(Serializer.Serialize(dto), remoteAddress, int.Parse(remotePort));
			}
			catch (Exception exc)
			{
				//show error?
			}
		}

		public static async Task<string> GetLocalIp()
		{
			var interfaces = await CommsInterface.GetAllInterfacesAsync();
			//TODO: check if any
			return interfaces.Last(i => !i.IsLoopback && i.IsUsable).IpAddress + ":" + Port;
		}
	}
}
