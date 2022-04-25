using Huobi.Net.Objects;
using Microsoft.Extensions.Logging;
using Huobi.Net.Clients;
using CryptoExchange.Net.Authentication;

namespace CaOrdersServer
{
	public class HuobSocket
	{
		HuobiSocketClient? _socketClient;
		User _user;

		public HuobSocket(User usr)
		{
			_user = usr;
			ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Huob" && k.IsWorking == true);
			if (keys == null) return;

			string key = keys.Key;
			string sec = keys.Secret;

			_socketClient = new HuobiSocketClient(new HuobiSocketClientOptions()
			{
				LogLevel = LogLevel.Trace,
				ApiCredentials = new ApiCredentials(key, sec)
			});
		}
		public bool InitOrdersListenerSpot()
		{
			bool b = false;
			if (_socketClient == null) return b;
			try
			{
				var res = _socketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
					null,
					onOrderUpdateMessage =>
					{
						Log.Write("OrderUpdate", _user.ID);
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
