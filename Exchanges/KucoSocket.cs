using Kucoin.Net.Objects;
using Microsoft.Extensions.Logging;
using Kucoin.Net.Clients;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Objects.Models.Spot.Socket;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
	public class KucoSocket : IApiSocket
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
		}
		public bool InitOrdersListener(int minutesToReconnect = 20)
		{
			bool b = false;
			try
			{
				if (_apiKey.IsWorking)
				{
					_socketClient = new KucoinSocketClient(
						new KucoinSocketClientOptions()
						{
							ApiCredentials = new KucoinApiCredentials(_apiKey.Key, _apiKey.Secret, _apiKey.PassPhrase),
							AutoReconnect = true,
							LogLevel = LogLevel.Trace
						});
					var res = _socketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
						onOrderData =>
						{
							KucoinStreamOrderBaseUpdate ord = onOrderData.Data;

							Order o = new Order(ord, _user.ID);
							if (o.state == OState.Open) 
							{
								// Для ордера, открытого через Socket, не могу понять он СПОТ или МАРЖ
								// используем Call запрос для получения инфы об ордере
								// это только для Кукоина
								o = _user.Exchanges.Find(e => e.Name == "Kuco")!.GetOrder(ord.OrderId);
							}
							o.Update("KucoSocket");

							OnMessage?.Invoke($"Kuco({_user.Name}): Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {o.state}|{ord.Status}");
						},
						onTradeData =>
						{
							KucoinStreamOrderMatchUpdate trd = onTradeData.Data;
							OnMessage?.Invoke($"Kuco({_user.Name}): Trade({trd.Symbol}/{trd.Side}/{trd.Price}) updated to {trd.Status}");
						}
					).Result;
					if (res.Success)
					{
						_socketSubscr = res.Data;
						OnMessage?.Invoke($"Kuco({_user.Name}): socket init ok");

						Task.Run(() => KeepAlive(minutesToReconnect));

						b = true;
					}
					else
					{
						string msg = $"Kuco({_user.Name}): Error in SubscribeToUserData: \r\n{res.Error?.Message}";
						OnMessage?.Invoke(msg);
						Log.Write(msg, _user.ID);
					}
				}
			}
			catch (Exception ex)
            {
				string msg = $"Kuco({_user.Name}): Exception in SubscribeToUserData: \r\n{ex.Message}";
				OnMessage?.Invoke(msg);
				Log.Write(msg, _user.ID);
			}
			return b;
		}
		public void KeepAlive(int minutesToReconnect = 20)
		{
			Thread.Sleep(minutesToReconnect * 60 * 1000);
			_socketSubscr?.ReconnectAsync();

			OnMessage?.Invoke($"Kuco({_user.Name}) socket reconnected");
			KeepAlive(minutesToReconnect);
		}

		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;

				OnMessage?.Invoke($"Kuco({_user.Name}) socket disposed");
			}
		}
	}
}
