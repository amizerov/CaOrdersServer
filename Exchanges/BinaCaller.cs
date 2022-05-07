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
        BinanceClient _binaClient = new();

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
                _binaClient = new BinanceClient(
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
                var r = _binaClient!.SpotApi.Account.GetAccountInfoAsync().Result;
                if (r != null && r.Success)
                {
                    balances = r.Data.Balances.ToList();
                }
            }
            return balances;
        }
        public BinanceOrder? GetOrder(string symbo, long oid)
        {
            if (_apiKey.IsWorking)
            {
                var res = _binaClient?.SpotApi.Trading.GetOrderAsync(symbo, oid).Result;
                if (res != null && res.Success)
                    return res.Data;
            }
            return null;
        }
        public List<BinanceOrder> GetAllOrdersSpot()
        {
            List<BinanceOrder> orders = new List <BinanceOrder>();
            if (_apiKey.IsWorking)
            {
                foreach (var symbo in GetSymbols())
                {
                    var r = _binaClient!.SpotApi.Trading.GetOrdersAsync(symbo).Result;
                    if (r.Success)
                    {
                        List<BinanceOrder> os = r.Data.ToList();
                        if (os.Count() > 0)
                            orders.AddRange(os);
                    }
                }
            }
            return orders;
        }

        public void CheckOrdersMarg()
        {
            if (_binaClient == null) return;
            OnProgress?.Invoke($"Bina({_user.Name}): check margin orders start");

            var r1 = _binaClient.SpotApi.Trading.GetOpenMarginOrdersAsync().Result;
            if (r1.Success)
            {
                foreach (BinanceOrder o in r1.Data)
                {
                    CaOrder order = new CaOrder(o, _user.ID, false/*marg*/);
                    order.Save();

                    OnProgress?.Invoke($"Bina({_user.Name}): margin Order {o.Id} - {o.Symbol} - state = {o.Status}");
                }
            }

            List<string> Symbols = GetSymbols();
            foreach (var symbo in Symbols)
            {
                var r2 = _binaClient.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
                if (r2.Success)
                {
                    foreach (var o in r2.Data)
                    {
                        if (o.Status == OrderStatus.New) continue;

                        CaOrder order = new CaOrder(o, _user.ID, false/*marg*/);
                        order.Save();

                        OnProgress?.Invoke($"Bina({_user.Name}): margin Order {o.Id} - {o.Symbol} - state = {o.Status}");
                    }
                }
            }

            OnProgress?.Invoke($"Bina({_user.Name}): margin orders done");
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
