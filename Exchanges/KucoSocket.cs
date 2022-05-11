using Kucoin.Net.Objects;
using Microsoft.Extensions.Logging;
using Kucoin.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Objects.Models.Spot.Socket;

namespace CaOrdersServer
{
	public class KucoSocket
	{
		public event Action<string>? OnMessage;

		User _user;
		ApiKey _apiKey;

		KucoinSocketClient? _socketClient;
		UpdateSubscription? _socketSubscrSpot;
		UpdateSubscription? _socketSubscrMarg;

		KucoinClient? _restClient;

		string _listenKeySpot = "";
		string _listenKeyMarg = "";

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
		public bool InitOrdersListener(bool spotMarg = true)
		{
			bool b = false;
			if (_socketClient == null) return b;

			var res = _socketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
				onOrderData =>
				{
					KucoinStreamOrderBaseUpdate ord = onOrderData.Data;
					new CaOrder(ord, _user.ID).Save();

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
				b = true;
				_socketSubscrSpot = res.Data;
				OnMessage?.Invoke($"Kucoin SubscribeToOrderUpdates is Ok");
			}
			else
				OnMessage?.Invoke($"Kucoin Error in SubscribeToOrderUpdates: {res.Error?.Message}");

			return b;
		}
		public bool KeepAlive(bool spotMarg = true)
		{
			UpdateSubscription? ups = spotMarg ? _socketSubscrSpot : _socketSubscrMarg;
			if (ups == null) return false;
			try
			{
				ups.ReconnectAsync();
			}
			catch
			{
				return false;
			}
			return true;
		}

		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;
			}
		}
	}
}
