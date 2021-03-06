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
		User _user;
		ApiKey? _apiKey;

		BinanceSocketClient? _socketClient;
		UpdateSubscription? _socketSubscrSpot;
		UpdateSubscription? _socketSubscrMarg;
		
		BinanceClient? _restClient;

		string _listenKeySpot = "";
		string _listenKeyMarg = "";

		public BinaSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exch == Exch.Bina);

			if (_apiKey != null && _apiKey.IsWorking)
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
					ApiCredentials = new ApiCredentials(_apiKey!.Key, _apiKey.Secret)
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
				Msg.Send(2, _user, Exch.Bina, "CreateListenKey",
					$"Create {(spotMarg ? "Spot" : "Marg")}  Listen Key failed - {res.Error?.Message}");
				
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
			if (_apiKey != null && _apiKey.IsWorking)
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

						Msg.Send(2, _user, Exch.Bina, "SubscribeToOrdersUpdates",
							$"{(spotMarg ? "Spot" : "Marg")} socket init ok");

						Task.Run(() => KeepAlive(minutesToReconnect));
					}
					else
					{
						Msg.Send(2, _user, Exch.Bina, "SubscribeToOrdersUpdates",
							$"Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}");

						return false;
					}
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		private void OnOrderUpdate(BinanceStreamOrderUpdate ord, bool spotMarg) 
		{
			bool newOrUpdated = new Order(ord, _user.ID, spotMarg).Update("BinaSocket");
			if (newOrUpdated)
				Msg.Send(2, _user, Exch.Bina, "OnOrderUpdate",
					$"New Order({ord.Symbol}/{ord.Side}/{ord.Price}) state {ord.Status}");
			else
				Msg.Send(2, _user, Exch.Bina, "OnOrderUpdate", 
					$"Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {ord.Status}");
		}
		private void OnAccountUpdate(BinanceStreamPositionsUpdate acc)
        {
			Msg.Send(2, _user, Exch.Bina, "OnAccountUpdate", "Account position updated");

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

			Msg.Send(2, _user, Exch.Bina, "OnAccountUpdate", "socket reconnected");

			KeepAlive(minutesToReconnect);
		}
		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if(setNull) _socketClient = null;

				Msg.Send(2, _user, Exch.Bina, "Dispose", "socket Disposed");
			}
		}
	}
}
