using CryptoExchange.Net.Authentication;
using Huobi.Net.Clients;
using Huobi.Net.Objects;
using Huobi.Net.Enums;
using Huobi.Net.Objects.Models;

namespace CaOrdersServer
{
    public class HuobCaller : ApiCaller
    {
        public event Action<string>? OnProgress;

        User _user;
        ApiKey _apiKey;
        HuobiClient _restClient = new();

        public HuobCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Huob") ?? new();

            CheckApiKey();
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey.ID > 0)
            {
                _restClient = new HuobiClient(
                    new HuobiClientOptions()
                    {
                        ApiCredentials = new ApiCredentials(_apiKey.Key!, _apiKey.Secret!)
                    });
                // Если получен доступ к балансам, ключ считается рабочим
                List<HuobiBalance> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;
            }
            OnProgress?.Invoke($"{_user.Name} - {_apiKey.Exchange} - IsWorking: {_apiKey.IsWorking}");
            return res;
        }
        public List<HuobiBalance> GetBalances()
        {
            List<HuobiBalance> balances = new();
            if (_apiKey.IsWorking)
            {
                var res = _restClient!.SpotApi.Account.GetAccountsAsync().Result;
                foreach (var acc in res.Data)
                {
                    var r = _restClient!.SpotApi.Account.GetBalancesAsync(acc.Id).Result;
                    if (r != null && r.Success)
                    {
                        balances.AddRange(r.Data.ToList());
                    }
                }
            }
            return balances;
        }

        public CaOrders GetOrders()
        {
            CaOrders orders = new(_user.ID);
            if (_apiKey.IsWorking)
            {
                var ro = _restClient.SpotApi.Trading.GetOpenOrdersAsync().Result;
                if (ro.Success)
                {
                    foreach (var o in ro.Data)
                    {
                        orders.Add(new CaOrder(o));
                    }
                }
                var r = _restClient.SpotApi.Trading.GetHistoricalOrdersAsync().Result;
                if (r.Success)
                {
                    foreach (var o in r.Data)
                    {
                        orders.Add(new CaOrder(o));
                    }
;                }
            }
            return orders;
        }
    }
}
