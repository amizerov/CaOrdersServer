using Binance.Net.Clients;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;

namespace CaOrdersServer
{
	public class BinaSocket
	{
		public event Action<string>? OnMessage;

		BinanceSocketClient? _socketClient;
		User _user;

		public BinaSocket(User usr)
		{
			_user = usr;
			ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Bina" && k.IsWorking == true);
			if (keys == null) return;

			string key = keys.Key;
			string sec = keys.Secret;

			BinanceSocketClientOptions opt = new BinanceSocketClientOptions();
			opt.LogLevel = LogLevel.Debug;

			if(key.Length > 10 && sec.Length > 10)
				opt.ApiCredentials = new ApiCredentials(key, sec);

			_socketClient = new BinanceSocketClient(opt);
		}
		public bool InitOrdersListenerSpot() 
		{
			bool b = false;
			if (_socketClient == null) return b;
			try
			{
				var res = _socketClient.SpotStreams.SubscribeToUserDataUpdatesAsync(
					_user.Name,
					onOrderUpdateMessage =>
					{
						Log.Write("OrderUpdate", _user.ID);
						OnMessage?.Invoke("OrderUpdate");
					},
					onOcoOrderUpdateMessage =>
					{
						Log.Write("OcoOrderUpdate", _user.ID);
						OnMessage?.Invoke("OcoOrderUpdate");
					},
					onAccountPositionMessage =>
					{
						Log.Write("AccountPosition", _user.ID);
						OnMessage?.Invoke("AccountPosition");
					},
					onAccountBalanceUpdateMessage =>
					{
						Log.Write("AccountBalanceUpda", _user.ID);
						OnMessage?.Invoke("AccountBalanceUpda");
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
				if(setNull) _socketClient = null;
			}
		}
	}
}
