using CryptoExchange.Net.Authentication;
using Huobi.Net.Clients;
using Huobi.Net.Objects;
using Huobi.Net.Enums;
using Huobi.Net.Objects.Models;

namespace CaOrdersServer
{
    public class HuobCaller : IApiCaller
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        User _user;
        ApiKey? _apiKey;
        HuobiClient _restClient = new();

        public HuobCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exch == Exch.Huob);
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey != null && _apiKey.ID > 0)
            {
                try 
                { 
                    _restClient = new HuobiClient(
                        new HuobiClientOptions()
                        {
                            ApiCredentials = new ApiCredentials(_apiKey.Key!, _apiKey.Secret!)
                        });
                }
                catch (Exception ex)
                {
                    OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "CheckApiKey", $" Error: {ex.Message}"));

                    return false;
                }
                // Если получен доступ к балансам, ключ считается рабочим
                List<HuobiBalance> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;

                OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "CheckApiKey", $"Key.IsWorking: {_apiKey.IsWorking}"));
            }

            return res;
        }
        List<HuobiBalance> GetBalances()
        {
            List<HuobiBalance> balances = new();
            var res = _restClient.SpotApi.Account.GetAccountsAsync().Result;
            if (res.Success)
            {
                foreach (var acc in res.Data)
                {
                    var r = _restClient.SpotApi.Account.GetBalancesAsync(acc.Id).Result;
                    if (r != null && r.Success)
                    {
                        balances.AddRange(r.Data.ToList());
                    }
                    else
                    {
                        OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "GetBalances", 
                            $"Error GetBalancesAsync: {res.Error?.Message}"));
                    }
                }
            }
            else
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "GetBalances",
                    $"Error GetAccountsAsync: {res.Error?.Message}"));
            }
            return balances;
        }
        public Order GetOrder(string oid, string symbol = "")
        {
            return new Order();
        }
        public Orders GetOrders()
        {
            Orders orders = new(_user); orders.OnProgress += Progress;

            if (_apiKey != null && _apiKey.IsWorking)
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "GetOrders", "GetOrders started"));

                var ro = _restClient.SpotApi.Trading.GetOpenOrdersAsync().Result;
                if (ro.Success)
                {
                    foreach (var o in ro.Data)
                    {
                        orders.Add(new Order(o));
                    }
                }
                var r = _restClient.SpotApi.Trading.GetHistoricalOrdersAsync().Result;
                if (r.Success)
                {
                    foreach (var o in r.Data)
                    {
                        orders.Add(new Order(o));
                    }
;                }
                OnProgress?.Invoke(new Message(1, _user, Exch.Huob, "GetOrders",
                    $"GetOrders orders.Count = {orders.Count}"));
            }
            return orders;
        }
    }
}
