using Huobi.Net.Objects;
using Microsoft.Extensions.Logging;
using Huobi.Net.Clients;
using CryptoExchange.Net.Authentication;
using Huobi.Net.Objects.Models.Socket;
using CryptoExchange.Net.Sockets;

namespace CaOrdersServer
{
	public class HuobSocket
	{
		public event Action<string>? OnMessage;

		User _user;
		ApiKey _apiKey;

		HuobiSocketClient? _socketClient;
		UpdateSubscription? _socketSubscrSpot;
		UpdateSubscription? _socketSubscrMarg;

		HuobiClient? _restClient;

		string _listenKeySpot = "";
		string _listenKeyMarg = "";

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
		public bool InitOrdersListener(bool spotMarg = true)
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
						new CaOrder(ord, _user.ID).Save();

						OnMessage?.Invoke($"Huobi: Order({ord.Symbol}/{ord.Side}) #{ord.OrderId} is update to {ord.Status} for User {_user.Name}");
					},
					onOcoOrderUpdateMessage =>
					{
						Log.Write("OcoOrderUpdate", _user.ID);
					},
					onAccountPositionMessage =>
					{
						Log.Write("AccountPosition", _user.ID);
					},
					onAccountBalanceUpdateMessage =>
					{
						Log.Write("AccountBalanceUpda", _user.ID);
					}
				).Result;
				if (res.Success)
					b = true;
				else
					Log.Write($"Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}", _user.ID);
			}
			catch (Exception ex)
			{
				Log.Write($"Exception in SubscribeToUserDataUpdatesAsync: {ex.Message}", _user.ID);
			}
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
