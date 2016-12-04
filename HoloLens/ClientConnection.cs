using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;

namespace Lightsaber.HoloLens
{
	public class ClientConnection
	{
		UdpSocketClient socketClient;
		Dictionary<Type, Action<object>> callbacks = new Dictionary<Type, Action<object>>();

		public bool Connected { get; private set; }

		public INetworkSerializer Serializer { get; set; }

		/// <summary>
		/// Connect to a client
		/// </summary>
		public async Task<bool> ConnectAsync(string ip, int port)
		{
			try
			{
				// if you need a duplex TCP sample, take a look at SmartHome sample
				socketClient = new UdpSocketClient();
				socketClient.MessageReceived += SocketClient_MessageReceived;
				Serializer = new ProtobufNetworkSerializer();
				await socketClient.ConnectAsync(ip, port);
				StartSendingKeepAlive();
				Connected = true;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		async void StartSendingKeepAlive()
		{
			while (true)
			{
				await socketClient.SendAsync(Serializer.Serialize(new PingDto { Message = "Ping!" }));
				await Task.Delay(2000).ConfigureAwait(false);
			}
		}

		void SocketClient_MessageReceived(object sender, Sockets.Plugin.Abstractions.UdpSocketMessageReceivedEventArgs e)
		{
			var dto = Serializer.Deserialize<BaseDto>(e.ByteData);
			Action<object> callback;
			lock (callbacks)
				callbacks.TryGetValue(dto.GetType(), out callback);
			callback?.Invoke(dto);
		}

		public void RegisterFor<T>(Action<T> callback)
		{
			lock (callbacks)
				callbacks[typeof(T)] = obj => callback((T) obj);
		}
	}
}
