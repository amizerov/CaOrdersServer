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
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        protected User _user;
        protected ApiKey? _apiKey;

        BinanceClient _restClient = new();

        public BinaCaller(User usr) { 
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exch == Exch.Bina);
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey != null)
            {
                try
                {
                    _restClient = new BinanceClient(
                        new BinanceClientOptions()
                        {
                            ApiCredentials = new ApiCredentials(_apiKey.Key!, _apiKey.Secret!)
                        });
                }
                catch (Exception ex)
                {
                    OnProgress?.Invoke(new Message(1, _user, Exch.Bina, "CheckApiKey", $"Error: {ex.Message}"));

                    return false;
                }
                // Если получен доступ к балансам, ключ считается рабочим
                List<BinanceBalance> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;
            
                OnProgress?.Invoke(new Message(1, _user, Exch.Bina, "CheckApiKey", $"Key.IsWorking: {_apiKey.IsWorking}"));
            }

            return res;
        }
        List<BinanceBalance> GetBalances()
        {
            List<BinanceBalance> balances = new();
            var res = _restClient!.SpotApi.Account.GetAccountInfoAsync().Result;
            if (res.Success)
            {
                balances = res.Data.Balances.ToList();
            }
            else
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Bina, "GetBalances", 
                    $"Error GetAccountInfoAsync: {res.Error?.Message}"));
            }
            return balances;
        }
        public Order GetOrder(string oid, string symbol = "") 
        {
            bool SPOT_MARG = true;
            Order order = new Order();
            var res = _restClient?.SpotApi.Trading.GetOrderAsync(symbol, long.Parse(oid)).Result;
            if(res != null && res.Success)
            {
                BinanceOrder o = res.Data;
                order = new Order(o, SPOT_MARG);
            }
            SPOT_MARG = false;
            res = _restClient?.SpotApi.Trading.GetMarginOrderAsync(symbol, long.Parse(oid)).Result;
            if (res != null && res.Success)
            {
                BinanceOrder o = res.Data;
                order = new Order(o, SPOT_MARG);
            }

            return order;
        }
        public Orders GetOrders()
        {
            Orders orders = new(_user); orders.OnProgress += Progress;

            if (_apiKey != null && _apiKey.IsWorking)
            {
                Progress(new Message(1, _user, Exch.Bina, "GetOrders", "Started"));

                List<string> symbols = GetUserTradedSymbols();
                foreach (var symbo in symbols)
                {
                    int cntSpot = 0, cntMarg = 0, ordersFound = 0;

                    var resSpot = _restClient!.SpotApi.Trading.GetOrdersAsync(symbo).Result;
                    if (resSpot.Success)
                    {
                        if (resSpot.Data != null)
                        {
                            cntSpot = resSpot.Data.Count();
                            foreach (var o in resSpot.Data)
                            {
                                orders.Add(new Order(o, true));
                                ordersFound = orders.Count;
                            }
                        }
                        else
                            Progress(new Message(1, _user, Exch.Bina, "GetOrders",
                                "SPOT {symbo} GetOrders Error: resSpot.Data == NULL"));
                    }
                    else
                        Progress(new Message(1, _user, Exch.Bina, "GetOrders", 
                            $"SPOT {symbo} GetOrders Error: {resSpot.Error?.Message}"));


                    if (IsAccountMargine())
                    {
                        var resMarg = _restClient!.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
                        if (resMarg.Success)
                        {
                            if (resMarg.Data != null)
                            {
                                cntMarg = resMarg.Data.Count();
                                foreach (var o in resMarg.Data)
                                {
                                    orders.Add(new Order(o, false));
                                    ordersFound = orders.Count;
                                }
                            }
                            else
                                Progress(new Message(1, _user, Exch.Bina, "GetOrders",
                                    $"MARG {symbo} GetMarginOrders Error: resMarg.Data == NULL"));
                        }
                        else
                            Progress(new Message(1, _user, Exch.Bina, "GetOrders",
                                $"MARG {symbo} GetMarginOrders Error: {resMarg.Error?.Message}"));
                    }
                    if (cntSpot + cntMarg > 0)
                        Progress(new Message(1, _user, Exch.Bina, $"GetOrders({symbo})", 
                            $"Spot:{cntSpot} Marg:{cntMarg} All:{ordersFound} orders found"));

                    Thread.Sleep(1000);
                    cntSpot = cntMarg = 0;
                }
                Progress(new Message(1, _user, Exch.Bina, "GetOrders",
                    $"Finished, orders.Count = {orders.Count}"));
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
