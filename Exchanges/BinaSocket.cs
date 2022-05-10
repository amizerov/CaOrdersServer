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
				OnMessage?.Invoke($"Create {(spotMarg ? "Spot" : "Marg")}  Listen Key for {_user.Name} failed - {res.Error?.Message}");
				return false;
            }
			return true;
		}
		public bool InitOrdersListener(bool spotMarg = true) 
		{
			if (CreateListenKey(spotMarg))
			{

				var listenKey = spotMarg ? _listenKeySpot : _listenKeyMarg;
				var res = _socketClient!.SpotStreams.SubscribeToUserDataUpdatesAsync(listenKey,
					onOrderUpdateMessage =>
					{
						BinanceStreamOrderUpdate ord = onOrderUpdateMessage.Data;

						bool newOrUpdated = _user.UpdateOrder(ord, spotMarg);
						if (newOrUpdated)
							OnMessage?.Invoke($"Binance: New Order #{ord.Id} added for user {_user.Name} in state {ord.Status}");
						else
							OnMessage?.Invoke($"Binance: Order #{ord.Id} of user {_user.Name} is updated to {ord.Status}");
					},
					null,
					onAccountPositionMessage =>
					{
						BinanceStreamPositionsUpdate acc = onAccountPositionMessage.Data;
						OnMessage?.Invoke($"Binance: Account position of user {_user.Name} is updated");

						_user.UpdateAccount(acc.Balances.ToList());
					},
					onAccountBalanceUpdateMessage =>
					{
						BinanceStreamBalanceUpdate acc = onAccountBalanceUpdateMessage.Data;
						OnMessage?.Invoke($"Binance: Account balance of user {_user.Name} is updated");

						_user.UpdateAccountBina();
					}
				).Result;
				if (res.Success)
				{
					if (spotMarg)
						_socketSubscrSpot = res.Data;
					else
						_socketSubscrMarg = res.Data;

					OnMessage?.Invoke($"Binance: {(spotMarg ? "Spot" : "Marg")} socket for {_user.Name} init ok");
				}
				else
				{
					string msg = $"Binance: Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}";
					OnMessage?.Invoke(msg);
					Log.Write(msg, _user.ID);
					return false;
				}
				return true;
			}
			else
				return false;
		}
		public bool KeepAlive(bool spotMarg = true)
        {
			UpdateSubscription? ups = spotMarg ? _socketSubscrSpot : _socketSubscrMarg;
			if (ups == null) return false;
			try
			{
				ups.ReconnectAsync();
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
