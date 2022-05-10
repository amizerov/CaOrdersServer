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

					bool newOrUpdated = _user.UpdateOrder(ord);
					Log.Write("onOrderData", _user.ID);
				},
				onTradeData =>
				{
					KucoinStreamOrderMatchUpdate trd = onTradeData.Data;
					Log.Write("onTradeData", _user.ID);
				}
			).Result;
			if (res.Success)
				b = true;
			else
				Log.Write($"Kucoin Error in SubscribeToOrderUpdatesAsync: {res.Error?.Message}", _user.ID);

			return b;
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
