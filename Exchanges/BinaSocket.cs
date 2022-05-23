using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CaOrdersServer
{
	public class BinaSocket : IApiSocket
	{
		public event Action<string>? OnMessage;

		User _user;
		ApiKey _apiKey;

		BinanceSocketClient? _socketClient;
		UpdateSubscription? _socketSubscrSpot;
		UpdateSubscription? _socketSubscrMarg;
		
		BinanceClient? _restClient;

		string _listenKeySpot = "";
		string _listenKeyMarg = "";

		public BinaSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exchange == "Bina") ?? new();

			if (_apiKey.IsWorking)
			{
				_socketClient = new BinanceSocketClient(
					new BinanceSocketClientOptions()
					{
						ApiCredentials = new ApiCredentials(_apiKey.Key, _apiKey.Secret),
						AutoReconnect = true,
						LogLevel = LogLevel.Trace
					});
			}
		}
		private bool CreateListenKey(bool spotMarg = true)
        {
			_restClient = new BinanceClient(
				new BinanceClientOptions()
				{
					ApiCredentials = new ApiCredentials(_apiKey.Key, _apiKey.Secret)
				});
			var res = spotMarg ? _restClient.SpotApi.Account.StartUserStreamAsync().Result
							   : _restClient.SpotApi.Account.StartMarginUserStreamAsync().Result;
			if (res.Success)
			{
				if (spotMarg)
					_listenKeySpot = res.Data;
				else
					_listenKeyMarg = res.Data;	
			}
            else
            {
				OnMessage?.Invoke($"Bina({_user.Name}): Create {(spotMarg ? "Spot" : "Marg")}  Listen Key failed - {res.Error?.Message}");
				return false;
            }
			return true;
		}
		public bool InitOrdersListener(int minutesToReconnect = 20)
        {
			return 
				SubscribeToOrdersUpdates(minutesToReconnect, true) && 
				SubscribeToOrdersUpdates(minutesToReconnect, false);

		}
		private bool SubscribeToOrdersUpdates(int minutesToReconnect = 20, bool spotMarg = true) 
		{
			if (CreateListenKey(spotMarg))
			{
				var lik = spotMarg ? _listenKeySpot : _listenKeyMarg; 
				var res = _socketClient!.SpotStreams.SubscribeToUserDataUpdatesAsync(
					lik,
					onOrderUpdateMessage => OnOrderUpdate(onOrderUpdateMessage.Data, spotMarg),
					null,
					onAccountPositionMessage => OnAccountUpdate(onAccountPositionMessage.Data),
					null
				).Result;
				if (res.Success)
				{
					if (spotMarg)
						_socketSubscrSpot = res.Data;
					else
						_socketSubscrMarg = res.Data; 

					OnMessage?.Invoke($"Bina({_user.Name}): {(spotMarg ? "Spot" : "Marg")} socket init ok");

					Task.Run(() => KeepAlive(minutesToReconnect));
				}
				else
				{
					string msg = $"Bina({_user.Name}): Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}";
					OnMessage?.Invoke(msg);
					Log.Write(msg, _user.ID);
					return false;
				}
				return true;
			}
			else
				return false;
		}
		private void OnOrderUpdate(BinanceStreamOrderUpdate ord, bool spotMarg) 
		{
			bool newOrUpdated = new Order(ord, _user.ID, spotMarg).Update("BinaSocket");
			if (newOrUpdated)
				OnMessage?.Invoke($"Bina({_user.Name}): New Order({ord.Symbol}/{ord.Side}/{ord.Price}) state {ord.Status}");
			else
				OnMessage?.Invoke($"Bina({_user.Name}): Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {ord.Status}");
		}
		private void OnAccountUpdate(BinanceStreamPositionsUpdate acc)
        {
			OnMessage?.Invoke($"Bina({_user.Name}): Account position updated");

			foreach (BinanceStreamBalance b in acc.Balances.ToList())
			{
				new CaBalance(b, _user.ID).Update();
			}
		}
		public void KeepAlive(int minutesToReconnect)
        {
			Thread.Sleep(minutesToReconnect * 60*1000);
			_socketSubscrSpot?.ReconnectAsync();
			_socketSubscrMarg?.ReconnectAsync();

			OnMessage?.Invoke($"Bina({_user.Name}) socket reconnected");
			KeepAlive(minutesToReconnect);
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
