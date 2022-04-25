using Kucoin.Net.Objects;
using Microsoft.Extensions.Logging;
using Kucoin.Net.Clients;
using CryptoExchange.Net.Authentication;

namespace CaOrdersServer
{
	public class KucoSocket
	{
		KucoinSocketClient? _socketClient;
		User _user;

		public KucoSocket(User usr)
		{
			_user = usr;
			ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Kuco" && k.IsWorking == true);
			if (keys == null) return;

			string key = keys.Key;
			string sec = keys.Secret;
			string pas = keys.PassPhrase;

			_socketClient = new KucoinSocketClient(new KucoinSocketClientOptions()
			{
				LogLevel = LogLevel.Trace,
				ApiCredentials = new KucoinApiCredentials(key, sec, pas)
			});
		}
		public bool InitOrdersListenerSpot()
		{
			bool b = false;
			if (_socketClient == null) return b;
			try
			{
				var res = _socketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
					onOrderData =>
					{
						Log.Write("onOrderData", _user.ID);
					},
					onTradeData =>
					{
						Log.Write("onTradeData", _user.ID);
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
