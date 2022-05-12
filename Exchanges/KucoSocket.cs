using Kucoin.Net.Objects;
using Microsoft.Extensions.Logging;
using Kucoin.Net.Clients;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Objects.Models.Spot.Socket;

namespace CaOrdersServer
{
	public class KucoSocket : ApiSocket
	{
		public event Action<string>? OnMessage;

		User _user;
		ApiKey _apiKey;

		KucoinSocketClient? _socketClient;
		UpdateSubscription? _socketSubscr;

		public KucoSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exchange == "Kuco") ?? new();

			if (_apiKey.IsWorking)
			{
				_socketClient = new KucoinSocketClient(
					new KucoinSocketClientOptions()
					{
						ApiCredentials = new KucoinApiCredentials(_apiKey.Key, _apiKey.Secret, _apiKey.Secret),
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
					onOrderData =>
					{
						KucoinStreamOrderBaseUpdate ord = onOrderData.Data;
						new CaOrder(ord, _user.ID).Update();

						OnMessage?.Invoke($"Kucoin: Order({ord.Symbol}/{ord.Side}) #{ord.OrderId} is update to {ord.Status} for User {_user.Name}");
					},
					onTradeData =>
					{
						KucoinStreamOrderMatchUpdate trd = onTradeData.Data;
						OnMessage?.Invoke($"Kucoin: Trade({trd.Symbol}/{trd.Side}) #{trd.OrderId} is update to {trd.Status} for User {_user.Name}");
					}
				).Result;
				if (res.Success)
				{
					_socketSubscr = res.Data;
					OnMessage?.Invoke($"Kucoin: socket for {_user.Name} init ok");

					Task.Run(() => KeepAlive(minutesToReconnect));

					b = true;
				}
				else
				{
					string msg = $"Kucoin({_user.Name}): Error in SubscribeToUserData: {res.Error?.Message}";
					OnMessage?.Invoke(msg);
					Log.Write(msg, _user.ID);
				}
			}
			catch (Exception ex)
            {
				string msg = $"Kucoin({_user.Name}): Exception in SubscribeToUserData: \r\n{ex.Message}";
				OnMessage?.Invoke(msg);
				Log.Write(msg, _user.ID);
			}
			return b;
		}
		public void KeepAlive(int minutesToReconnect = 20)
		{
			Thread.Sleep(minutesToReconnect * 60 * 1000);
			_socketSubscr?.ReconnectAsync();

			OnMessage?.Invoke("Kucoin socket reconnected");
			KeepAlive(minutesToReconnect);
		}

		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;

				OnMessage?.Invoke("Kucoin socket disposed");
			}
		}
	}
}
