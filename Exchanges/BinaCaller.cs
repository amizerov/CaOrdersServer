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
        ApiKey? _apiKey;
        BinanceClient? client;

        public BinaCaller(User usr) { 
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Bina");

            CheckApiKey();
        }
        public bool CheckApiKey() {
            bool b = false;
            if (_apiKey == null || String.IsNullOrEmpty(_apiKey.Key) || String.IsNullOrEmpty(_apiKey.Secret)) return b;

            client = new BinanceClient(
                new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_apiKey.Key!, _apiKey.Secret!)
                });
            var r = client.SpotApi.Account.GetAccountInfoAsync().Result;
            if(r != null && r.Success)
            {
                BinanceAccountInfo ai = r.Data;
                b = ai != null;
            }
            _apiKey.IsWorking = b;
            return b;
        }
        public void CheckOrdersSpot()
        {
            if (client == null || _apiKey == null || !_apiKey.IsWorking) return;

            OnProgress?.Invoke($"Bina({_user.Name}): check spot orders start");

            // Только открытые ордера
            var r1 = client.SpotApi.Trading.GetOpenOrdersAsync().Result;
            if (r1.Success)
            {
                foreach (BinanceOrder o in r1.Data)
                {
                    CaOrder order = new CaOrder(o, _user.ID);
                    order.Save();

                    OnProgress?.Invoke($"Bina({_user.Name}): Spot Open Order {o.Id} - {o.Symbol} - {o.CreateTime} - {o.Status}");
                }
            }

            // Все остальные, кроме открытых
            List<string> Symbols = GetSymbols();
            foreach (var symbo in Symbols)
            {
                var r2 = client.SpotApi.Trading.GetOrdersAsync(symbo).Result;
                if (r2.Success)
                {
                    foreach (BinanceOrder o in r2.Data)
                    {
                        if (o.Status == OrderStatus.New) continue;

                        CaOrder order = new CaOrder(o, _user.ID);
                        order.Save();

                        OnProgress?.Invoke($"Bina({_user.Name}): spot Order {o.Id} - {o.Symbol} - state = {o.Status} / {order.state}");
                    }
                }
            }

            OnProgress?.Invoke($"Bina({_user.Name}): spot orders done");
        }
        public void CheckOrdersMarg()
        {
            if (client == null) return;
            OnProgress?.Invoke($"Bina({_user.Name}): check margin orders start");

            var r1 = client.SpotApi.Trading.GetOpenMarginOrdersAsync().Result;
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
                var r2 = client.SpotApi.Trading.GetMarginOrdersAsync(symbo).Result;
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
        public void CheckOrdersSpotNewOnly()
        {
            if (client == null || _apiKey == null || !_apiKey.IsWorking) return;

            // Проверить статус открытых уже ранее
            using (var db = new CaDbConnector())
            {
                List<CaOrder> orders = db.Orders!
                    .Where(o => o.exchange == 1 && o.usr_id == _user.ID && o.state == 1 && o.spotmar == true).ToList();
                foreach (var order in orders)
                {
                    var res = client.SpotApi.Trading.GetOrderAsync(order.symbol!, long.Parse(order.ord_id!)).Result;
                    if (res.Success)
                    {
                        BinanceOrder o = res.Data;
                        order.state =
                            o.Status == OrderStatus.New ? (int)OState.Open :
                            o.Status == OrderStatus.Filled ? (int)OState.Filled :
                            o.Status == OrderStatus.Canceled ? (int)OState.Canceled :
                            o.Status == OrderStatus.Rejected ? (int)OState.Canceled :
                            o.Status == OrderStatus.Expired ? (int)OState.Canceled :
                            o.Status == OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.Error;

                        if (o.Status == OrderStatus.Filled) order.dt_exec = o.UpdateTime;
                        
                        db.SaveChanges();

                        OnProgress?.Invoke($"Bina({_user.Name}): Spot Open Order {o.Id} - {o.Symbol} - {o.CreateTime} - New to {o.Status}");
                    }
                    else
                    {
                        OnProgress?.Invoke($"Bina({_user.Name}): Spot Open Order Error {order.ord_id} - {res.Error?.Message}");
                    }
                }
            }

            // Записать новые открытые
            // Только открытые ордера
            var r1 = client.SpotApi.Trading.GetOpenOrdersAsync().Result;
            if (r1.Success)
            {
                foreach (BinanceOrder o in r1.Data)
                {
                    CaOrder order = new CaOrder(o, _user.ID);
                    order.Save();

                    OnProgress?.Invoke($"Bina({_user.Name}): Spot Open Order {o.Id} - {o.Symbol} - {o.CreateTime} - {o.Status}");
                }
            }
        }
        public void CheckOrdersMargNewOnly()
        {
            if (client == null || _apiKey == null || !_apiKey.IsWorking) return;

            // Проверить статус открытых уже ранее
            using (var db = new CaDbConnector())
            {
                List<CaOrder> orders = db.Orders!
                    .Where(o => o.exchange == 1 && o.usr_id == _user.ID && o.state == 1 && o.spotmar == false).ToList();
                foreach (var order in orders)
                {
                    var res = client.SpotApi.Trading.GetMarginOrderAsync(order.symbol!, long.Parse(order.ord_id!)).Result;
                    if (res.Success)
                    {
                        BinanceOrder o = res.Data;
                        order.state =
                            o.Status == OrderStatus.New ? (int)OState.Open :
                            o.Status == OrderStatus.Filled ? (int)OState.Filled :
                            o.Status == OrderStatus.Canceled ? (int)OState.Canceled :
                            o.Status == OrderStatus.Rejected ? (int)OState.Canceled :
                            o.Status == OrderStatus.Expired ? (int)OState.Canceled :
                            o.Status == OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.Error;

                        if (o.Status == OrderStatus.Filled) order.dt_exec = o.UpdateTime;

                        db.SaveChanges();

                        OnProgress?.Invoke($"Bina({_user.Name}): Marg Open Order {o.Id} - {o.Symbol} - {o.CreateTime} - New to {o.Status}");
                    }
                    else
                    {
                        OnProgress?.Invoke($"Bina({_user.Name}): Marg Open Order Error {order.ord_id} - {res.Error?.Message}");
                    }
                }
            }

            // Записать новые открытые
            // Только открытые ордера
            var r1 = client.SpotApi.Trading.GetOpenMarginOrdersAsync().Result;
            if (r1.Success)
            {
                foreach (BinanceOrder o in r1.Data)
                {
                    CaOrder order = new CaOrder(o, _user.ID);
                    order.Save();

                    OnProgress?.Invoke($"Bina({_user.Name}): Marg Open Order {o.Id} - {o.Symbol} - {o.CreateTime} - {o.Status}");
                }
            }
        }
        public List<string> GetSymbols()
        {
            List<string> sl = new List<string>();
            string sql = "select symbol, count(*) c, max(dt_create) d from Orders where exchange = 1 and usr_id = 4 group by symbol order by c desc";
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
