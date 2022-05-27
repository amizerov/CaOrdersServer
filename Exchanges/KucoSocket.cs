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
		public event Action<Message>? OnProgress;

		User _user;
		ApiKey _apiKey;

		KucoinSocketClient? _socketClient;
		UpdateSubscription? _socketSubscr;

		public KucoSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exchange == Exch.Kuco) ?? new();
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

							OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, "InitOrdersListener",
								$"Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {o.state}|{ord.Status}"));
						},
						onTradeData =>
						{
							KucoinStreamOrderMatchUpdate trd = onTradeData.Data;
							OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, "InitOrdersListener", 
								$"Trade({trd.Symbol}/{trd.Side}/{trd.Price}) updated to {trd.Status}"));
						}
					).Result;
					if (res.Success)
					{
						_socketSubscr = res.Data;
						OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, 
							"InitOrdersListener", $"Kuco({_user.Name}): socket init ok"));

						Task.Run(() => KeepAlive(minutesToReconnect));

						b = true;
					}
					else
					{
						OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, 
							"InitOrdersListener", $"Error in SubscribeToUserData: \r\n{res.Error?.Message}"));
					}
				}
			}
			catch (Exception ex)
            {
				OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, 
					"InitOrdersListener", $"Exception in SubscribeToUserData: \r\n{ex.Message}"));
			}
			return b;
		}
		public void KeepAlive(int minutesToReconnect = 20)
		{
			Thread.Sleep(minutesToReconnect * 60 * 1000);
			_socketSubscr?.ReconnectAsync();

			OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, 
				"KeepAlive", $"Kuco({_user.Name}) socket reconnected"));

			KeepAlive(minutesToReconnect);
		}

		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;

				OnProgress?.Invoke(new Message(2, _user, Exch.Kuco, 
					"Dispose", $"Kuco({_user.Name}) socket disposed"));
			}
		}
	}
}
