using Huobi.Net.Objects;
using Microsoft.Extensions.Logging;
using Huobi.Net.Clients;
using CryptoExchange.Net.Authentication;
using Huobi.Net.Objects.Models.Socket;
using CryptoExchange.Net.Sockets;

namespace CaOrdersServer
{
	public class HuobSocket : IApiSocket
	{
		public event Action<string>? OnMessage;

		User _user;
		ApiKey _apiKey;

		HuobiSocketClient? _socketClient;
		UpdateSubscription? _socketSubscr;

		public HuobSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exchange == "Kuco") ?? new();

			if (_apiKey.IsWorking)
			{
				_socketClient = new HuobiSocketClient(
					new HuobiSocketClientOptions()
					{
						ApiCredentials = new ApiCredentials(_apiKey.Key, _apiKey.Secret),
						AutoReconnect = true,
						LogLevel = LogLevel.Trace
					});
			}
		}
		public bool InitOrdersListener(int minutesToReconnect = 20)
		{
			bool b = false;
			if (_socketClient == null) return b;
			try
			{
				var res = _socketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
					null,
					onOrderUpdateMessage =>
					{
						HuobiSubmittedOrderUpdate ord = onOrderUpdateMessage.Data;
						new Order(ord, _user.ID).Update("HuobSocket");

						OnMessage?.Invoke($"Huob({_user.Name}): Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {ord.Status}");
					}
				).Result;
				if (res.Success)
				{
					_socketSubscr = res.Data;
					OnMessage?.Invoke($"Huob({_user.Name}): socket init ok");

					Task.Run(() => KeepAlive(minutesToReconnect));

					b = true;
				}
				else
				{
					string msg = $"Huob({_user.Name}): Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}";
					OnMessage?.Invoke(msg);
					Log.Write(msg, _user.ID);
				}
			}
			catch (Exception ex)
			{
				string msg = $"Huob({_user.Name}): Exception in SubscribeToUserDataUpdatesAsync: {ex.Message}";
				OnMessage?.Invoke(msg);
				Log.Write(msg, _user.ID);
			}
			return b;
		}
		public void KeepAlive(int minutesToReconnect = 20)
        {
			Thread.Sleep(minutesToReconnect * 60 * 1000);
			_socketSubscr?.ReconnectAsync();

			OnMessage?.Invoke($"Huob({_user.Name}) socket reconnected");
			KeepAlive(minutesToReconnect);
		}
		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;

				OnMessage?.Invoke($"Huob({_user.Name}) socket disposed");
			}
		}
	}
}
