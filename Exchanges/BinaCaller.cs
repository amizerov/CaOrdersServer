using am.BL;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Authentication;
using System.Data;

namespace CaOrdersServer
{
    public class BinaCaller
    {
        public event Action<string>? OnProgress;

        User _user;
        ApiKey _apiKey;
        BinanceClient _restClient = new();

        public BinaCaller(User usr) { 
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Bina") ?? new();

            CheckApiKey();
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
            OnProgress?.Invoke($"{_user.Name} - {_apiKey.Exchange} - IsWorking: {_apiKey.IsWorking}");
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
        public List<CaOrder> GetAllOrders(bool spotMarg = true)
        {
            List<CaOrder> orders = new();
            if (_apiKey.IsWorking)
            {
                foreach (var symbo in GetSymbols())
                {
                    var r = spotMarg ? _restClient!.SpotApi.Trading.GetOrdersAsync(symbo).Result
                                     : _restClient!.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
                    if (r.Success)
                    {
                        foreach(var o in r.Data)
                        {
                            orders.Add(new CaOrder(o, _user.ID, spotMarg));
                        }
                    }
                }
            }
            return orders;
        }
        public List<string> GetSymbols()
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
