using Huobi.Net.Objects;
using Microsoft.Extensions.Logging;
using Huobi.Net.Clients;
using CryptoExchange.Net.Authentication;
using Huobi.Net.Objects.Models.Socket;
using CryptoExchange.Net.Sockets;

namespace CaOrdersServer
{
	public class HuobSocket : IApiSocket
	{
		User _user;
		ApiKey? _apiKey;

		HuobiSocketClient? _socketClient;
		UpdateSubscription? _socketSubscr;

		public HuobSocket(User usr)
		{
			_user = usr;
			_apiKey = _user.ApiKeys.Find(k => k.Exch == Exch.Kuco);

			if (_apiKey != null && _apiKey.IsWorking)
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
		public bool InitOrdersListener(int minutesToReconnect = 20)
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
						new Order(ord, _user.ID).Update("HuobSocket");

					Msg.Send(2, _user, Exch.Huob, "InitOrdersListener",
							$"Order({ord.Symbol}/{ord.Side}/{ord.Price}) updated to {ord.Status}");
					}
				).Result;
				if (res.Success)
				{
					_socketSubscr = res.Data;
					Msg.Send(2, _user, Exch.Huob, "InitOrdersListener", "socket init ok");

					Task.Run(() => KeepAlive(minutesToReconnect));

					b = true;
				}
				else
				{
					Msg.Send(2, _user, Exch.Huob, "InitOrdersListener", 
						$"Error in SubscribeToUserDataUpdatesAsync: {res.Error?.Message}");
				}
			}
			catch (Exception ex)
			{
				Msg.Send(2, _user, Exch.Huob, "InitOrdersListener", 
					$"Exception in SubscribeToUserDataUpdatesAsync: {ex.Message}");
			}
			return b;
		}
		public void KeepAlive(int minutesToReconnect = 20)
        {
			Thread.Sleep(minutesToReconnect * 60 * 1000);
			_socketSubscr?.ReconnectAsync();

			Msg.Send(2, _user, Exch.Huob, "KeepAlive", "socket reconnected");
			KeepAlive(minutesToReconnect);
		}
		public void Dispose(bool setNull = true)
		{
			if (_socketClient != null)
			{
				_socketClient.UnsubscribeAllAsync();
				if (setNull) _socketClient = null;

				Msg.Send(2, _user, Exch.Huob, "Dispose", "socket disposed");
			}
		}
	}
}
