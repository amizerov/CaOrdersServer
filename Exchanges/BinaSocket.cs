using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CaOrdersServer
{
	public class BinaSocket
	{
		public event Action<string>? OnMessage;

		bool _keyIsWorking = false;
		string _key = "", _sec = "";
		BinanceSocketClient? _socketClient;
		BinanceClient? _restClient;
		UpdateSubscription? _socketSubscr;

		string _listenKey = ""; 
		User _user;

		public BinaSocket(User usr)
		{
			_user = usr;
			ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Bina" && k.IsWorking == true);
			if (keys == null) return;
			_key = keys.Key;
			_sec = keys.Secret;

			_socketClient = new BinanceSocketClient(
				new BinanceSocketClientOptions() { 
					ApiCredentials = new ApiCredentials(_key, _sec), 
					AutoReconnect = true,
					LogLevel = LogLevel.Trace 
				});

			if(_socketClient != null)
				_keyIsWorking = keys.IsWorking;

		}
		private bool CreateListenKeyForSocket()
        {
			if (!_keyIsWorking) return false;

			_restClient = new BinanceClient(
				new BinanceClientOptions()
				{
					ApiCredentials = new ApiCredentials(_key, _sec)
				});
			var listenKey = _restClient.SpotApi.Account.StartUserStreamAsync().Result;
			if (!listenKey.Success) return false;
			_listenKey = listenKey.Data;

			return true;
		}
		public bool InitOrdersListenerSpot() 
		{
			if(!CreateListenKeyForSocket()) return false;

			try
			{
				var res = _socketClient!.SpotStreams.SubscribeToUserDataUpdatesAsync(_listenKey,
					onOrderUpdateMessage =>
					{
						BinanceStreamOrderUpdate ord = onOrderUpdateMessage.Data;
						OnMessage?.Invoke($"Order #{ord.Id} of user {_user.Name} is updated to {ord.Status}");
					
						_user.UpdateOrder(ord.Symbol, ord.Id);
					},
					null,
					onAccountPositionMessage =>
					{
						BinanceStreamPositionsUpdate acc =onAccountPositionMessage.Data;
						OnMessage?.Invoke($"Account position of user {_user.Name} is updated");

						_user.UpdateAccount();
					},
					onAccountBalanceUpdateMessage =>
					{
						BinanceStreamBalanceUpdate acc = onAccountBalanceUpdateMessage.Data;
						OnMessage?.Invoke($"Account balance of user {_user.Name} is updated");

						_user.UpdateAccount();
					}
				).Result;
				if (res.Success) {
					_socketSubscr = res.Data;
				}
				else {
					Log.Write($"Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}", _user.ID);
					return false; 
				}
			}
			catch (Exception ex)
            {
				Log.Write($"Exception in SubscribeToUserDataUpdatesAsync: {ex.Message}", _user.ID);
				return false;
			}
			return true;
		}
		public bool KeepAliveSocket()
        {
			if (_socketSubscr == null) return false;
			try
			{
				_socketSubscr.ReconnectAsync();
			}
			catch { 
				return false;
			}
			return true;
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
