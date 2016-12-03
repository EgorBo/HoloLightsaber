using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;

namespace Lightsaber.HoloLens
{
	public class ClientConnection
	{
		TcpSocketClient socketClient;
		readonly Dictionary<string, BaseDto> objectsToSend = new Dictionary<string, BaseDto>();
		Dictionary<Type, Action<object>> callbacks = new Dictionary<Type, Action<object>>();

		/// <summary>
		/// Fired when conneciton is closed.
		/// </summary>
		public event Action Disconnected;

		/// <summary>
		/// Is connected to uwp/android/ios app
		/// </summary>
		public bool Connected { get; private set; }


		public INetworkSerializer Serializer { get; set; }

		/// <summary>
		/// Connect to a client
		/// </summary>
		public async Task<bool> ConnectAsync(string ip, int port)
		{
			try
			{
				socketClient = new TcpSocketClient();
				socketClient.Socket.Control.NoDelay = true;
				//socketClient.Socket.Control.OutboundBufferSizeInBytes = 32;
				socketClient.Socket.Control.QualityOfService = Windows.Networking.Sockets.SocketQualityOfService.LowLatency;

				Serializer = new ProtobufNetworkSerializer();
				await socketClient.ConnectAsync(ip, port);
				Connected = true;
				Task.Run(() => StartListening());
			}
			catch (Exception)
			{
				return false;
			}
			StartSendingData();
			return true;
		}

		void StartListening()
		{
			try
			{
				Serializer.ObjectDeserialized += OnObjectDeserialized;
				Serializer.ReadFromStream(socketClient.ReadStream);
			}
			catch (Exception exc)
			{
			}
		}

		void OnObjectDeserialized(BaseDto obj)
		{
			if (obj == null)
				return;

			lock (callbacks)
			{
				Action<object> callback;
				if (callbacks.TryGetValue(obj.GetType(), out callback))
				{
					callback(obj);
				}
			}
		}

		public void RegisterFor<T>(Action<T> callback)
		{
			lock (callbacks)
			{
				callbacks[typeof(T)] = obj => callback((T)obj);
			}
		}

		public void SendObject(string id, BaseDto dto)
		{
			lock (objectsToSend)
				objectsToSend[id] = dto;
		}

		public void SendObject(BaseDto dto)
		{
			SendObject(Guid.NewGuid().ToString(), dto);
		}

		async void StartSendingData()
		{
			try
			{
				await Task.Run(async () =>
				{
					while (true)
					{
						List<BaseDto> objects;

						lock (objectsToSend)
						{
							objects = objectsToSend.Values.ToList();
							objectsToSend.Clear();
						}

						if (objects.Count > 0)
						{
							foreach (var item in objects)
							{
								Serializer.WriteToStream(socketClient.WriteStream, item);
							}
						}
						await Task.Delay(20);
					}
				});
			}
			catch (Exception exc)
			{
				Connected = false;
				Disconnected?.Invoke();
			}
		}
	}
}
