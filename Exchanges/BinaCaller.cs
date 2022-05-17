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

                List<string> symbols = GetUserTradedSymbols();
                foreach (var symbo in symbols)
                {
                    bool orderFound = false;
                    var resSpot = _restClient!.SpotApi.Trading.GetOrdersAsync(symbo).Result;
                    if (resSpot.Success)
                    {
                        if(resSpot.Data != null)
                            foreach(var o in resSpot.Data)
                            {
                                orders.Add(new Order(o, true));
                                orderFound = true;
                            }
                        else
                            OnProgress?.Invoke($"Binance({_user.Name}): SPOT {symbo} resSpot.Data == NULL");
                    }
                    else
                        OnProgress?.Invoke($"Binance({_user.Name}): SPOT {symbo} Error: {resSpot.Error?.Message}");


                    if (IsAccountMargine())
                    {
                        var resMarg = _restClient!.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
                        if (resMarg.Success)
                        {
                            if (resMarg.Data != null)
                                foreach (var o in resMarg.Data)
                                {
                                    orders.Add(new Order(o, false));
                                    orderFound = true;
                                }
                            else
                                OnProgress?.Invoke($"Binance({_user.Name}): MARG {symbo} resMarg.Data == NULL");
                        }
                        else
                            OnProgress?.Invoke($"Binance({_user.Name}): MARG {symbo} Error: {resMarg.Error?.Message}");
                    }
                    if (orderFound)
                        OnProgress?.Invoke($"Binance({_user.Name}): orders found for {symbo}");

                    Thread.Sleep(1000);
                }
                OnProgress?.Invoke($"Binance({_user.Name}): GetOrders orders.Count = {orders.Count}");
            }
            return orders;
        }
        private List<string> GetUserTradedSymbols()
        {
            List<string> sl = new List<string>();

            string sql = $"select distinct symbol from Orders where exchange = 1 and usr_id = {_user.ID}";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                var s = G._S(r["symbol"]);
                if(!sl.Contains(s)) sl.Add(s);
            }

            if(sl.Count == 0) sl = GetAllBinanceSymbols();

            return sl;
        }
        private List<string> GetAllBinanceSymbols()
        {
            List<string> sl = new List<string>();

            string cdt = DateTime.Now.ToString("yyyyMMdd");
            string sql = $"select distinct symbol from Products where exchange = 1 and dtc > '{cdt}'";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                var s = G._S(r["symbol"]);
                sl.Add(s);
            }

            return sl;            
        }
        private bool IsAccountMargine()
        {
            // TODO: не работает
            bool isMargAcc = true; // false;
            
            var resAcco = _restClient!.SpotApi.Account.GetAccountInfoAsync().Result;
            if (resAcco.Success)
                foreach (var p in resAcco.Data.Permissions)
                    if (p == Binance.Net.Enums.AccountType.Margin) 
                        isMargAcc = true;

            return isMargAcc;
        }
    }
}
