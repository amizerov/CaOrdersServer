using am.BL;
using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Authentication;
using System.Data;

namespace CaOrdersServer
{
    public class BinaCaller : IApiCaller
    {
        public event Action<string>? OnProgress;

        protected User _user;
        protected ApiKey _apiKey;

        BinanceClient _restClient = new();

        public BinaCaller(User usr) { 
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Bina") ?? new();
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey.ID > 0)
            {
                _restClient = new BinanceClient(
                    new BinanceClientOptions()
                    {
                        ApiCredentials = new ApiCredentials(_apiKey.Key!, _apiKey.Secret!)
                    });
                // Если получен доступ к балансам, ключ считается рабочим
                List<BinanceBalance> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;
            }
            OnProgress?.Invoke($"CheckApiKey({_apiKey.Exchange}/{_user.Name}) Key.IsWorking: {_apiKey.IsWorking}");
            return res;
        }
        public List<BinanceBalance> GetBalances()
        {
            List<BinanceBalance> balances = new();
            if (_apiKey.IsWorking)
            {
                var r = _restClient!.SpotApi.Account.GetAccountInfoAsync().Result;
                if (r != null && r.Success)
                {
                    balances = r.Data.Balances.ToList();
                }
            }
            return balances;
        }
        public Orders GetOrders()
        {
            Orders orders = new(_user);
            if (_apiKey.IsWorking)
            {
                OnProgress?.Invoke($"Binance({_user.Name}): GetOrders started");

                foreach (var symbo in GetSymbols())
                {
                    var resSpot = _restClient!.SpotApi.Trading.GetOrdersAsync(symbo).Result;
                    if (resSpot.Success)
                    {
                        foreach(var o in resSpot.Data)
                        {
                            orders.Add(new Order(o, true));
                        }
                    }
                    var resMarg = _restClient!.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
                    if (resMarg.Success)
                    {
                        foreach (var o in resMarg.Data)
                        {
                            orders.Add(new Order(o, false));
                        }
                    }
                }
                OnProgress?.Invoke($"Binance({_user.Name}): GetOrders orders.Count = {orders.Count}");
            }
            return orders;
        }
        private List<string> GetSymbols()
        {
            List<string> sl = new List<string>();
            string sql = $"select distinct symbol from Orders where exchange = 1 and usr_id = {_user.ID}";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                var s = G._S(r["symbol"]);
                if(!sl.Contains(s)) sl.Add(s);
            }
            return sl;
        }
    }
}
