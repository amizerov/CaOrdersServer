using CryptoExchange.Net.Authentication;
using Huobi.Net.Clients;
using Huobi.Net.Objects;
using Huobi.Net.Enums;
using Huobi.Net.Objects.Models;

namespace CaOrdersServer
{
    public class HuobCaller : IApiCaller
    {
        public event Action<string>? OnProgress;

        User _user;
        ApiKey _apiKey;
        HuobiClient _restClient = new();

        public HuobCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Huob") ?? new();
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey.ID > 0)
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
                    OnProgress?.Invoke($"CheckApiKey({_apiKey.Exchange}/{_user.Name}) Error: {ex.Message}");
                    return false;
                }
                // Если получен доступ к балансам, ключ считается рабочим
                List<HuobiBalance> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;
            }
            OnProgress?.Invoke($"CheckApiKey({_apiKey.Exchange}/{_user.Name}) Key.IsWorking: {_apiKey.IsWorking}");
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
                        OnProgress?.Invoke($"Huob({_user.Name}) Error GetBalancesAsync: {res.Error?.Message}");
                    }
                }
            }
            else
            {
                OnProgress?.Invoke($"Huob({_user.Name}) Error GetAccountsAsync: {res.Error?.Message}");
            }
            return balances;
        }
        public Order GetOrder(string oid)
        {
            return new Order();
        }
        public Orders GetOrders()
        {
            Orders orders = new(_user);
            if (_apiKey.IsWorking)
            {
                OnProgress?.Invoke($"Huob({_user.Name}): GetOrders started");

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
                OnProgress?.Invoke($"Huob({_user.Name}): GetOrders orders.Count = {orders.Count}");
            }
            return orders;
        }
    }
}
