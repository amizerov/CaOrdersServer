using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Authentication;

namespace CaOrdersServer
{
    public class BinaCaller
    {
        public event Action<string>? OnProgress;

        User _user;
        string? _key, _sec;

        public BinaCaller(User usr) { 
            _user = usr;
            ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Bina");
            if (keys == null) return;

            _key = keys.Key;
            _sec = keys.Secret;
        }
        public bool CheckApiKey() {
            bool b = false; if (_key == null) return b;
            BinanceClient client = new BinanceClient(
                new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_key!, _sec!)
                });
            var r = client.SpotApi.Account.GetAccountInfoAsync().Result;
            if(r != null && r.Success)
            {
                BinanceAccountInfo ai = r.Data;
                b = true;
            }

            return b;
        }
        public void CheckOrdersSpot()
        {
            if (_key == null) return;
            OnProgress?.Invoke($"Bina({_user.Name}): check spot orders start");

            BinanceClient client = new BinanceClient(
                new BinanceClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_key!, _sec!)
                });

            CaOrders OpenOrders = new CaOrders(_user.ID, 1/*Bina*/);
            var ro = client.SpotApi.Trading.GetOpenOrdersAsync().Result;
            if (ro.Success)
            {
                foreach (var o in ro.Data)
                {
                    CaOrder order = new CaOrder();
                    order.usr_id = _user.ID;

                    order.ord_id = o.Id.ToString();
                    order.exchange = 1; // 1 - Bina, 2 - Kuco, 3 - Huob
                    order.symbol = o.Symbol;
                    order.spotmar = true;
                    order.buysel = o.Side == OrderSide.Buy;
                    order.price = o.Price;
                    order.qty = o.Quantity;
                    order.dt_create = o.CreateTime;
                    order.state = (int)OState.Open;
                    
                    order.Save();
                    OpenOrders.Add(order);

                    OnProgress?.Invoke($"Bina({_user.Name}): Opened spot order {o.Id} - {o.Symbol} - {o.Price}");
                }
            }
            // TODO: надо переделать внешний цикл по всем символам,
            // по которым были ордера этого юзера в базе, не только открытые
            CaOrders orders2 = new CaOrders(_user.ID, 1/*Bina*/);
            foreach (var ord in OpenOrders){
                var r = client.SpotApi.Trading.GetOrdersAsync(ord.symbol!).Result;
                if (r.Success)
                {
                    foreach (var o in r.Data)
                    {
                        CaOrder order = new CaOrder();
                        order.usr_id = _user.ID;

                        order.ord_id = o.Id.ToString();
                        order.exchange = 1; // 1 - Bina, 2 - Kuco, 3 - Huob
                        order.symbol = o.Symbol;
                        order.spotmar = true;
                        order.buysel = o.Side == OrderSide.Buy;
                        order.price = o.Price;
                        order.qty = o.Quantity;
                        order.dt_create = o.CreateTime;
                        order.state = o.Status == OrderStatus.New ? (int)OState.Open :
                                      o.Status == OrderStatus.Filled ? (int)OState.Filled :
                                      o.Status == OrderStatus.Canceled ? (int)OState.Canceled :
                                      o.Status == OrderStatus.Rejected ? (int)OState.Canceled :
                                      o.Status == OrderStatus.Expired ? (int)OState.Canceled :
                                      o.Status == OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.NotFound;

                        if (o.Status == OrderStatus.Filled)
                            order.dt_exec = o.UpdateTime;

                        order.Save();
                        orders2.Add(order);

                        OnProgress?.Invoke($"Bina({_user.Name}): Order {o.Id} - {o.Symbol} - state = {o.Status}");
                    }
                }
            }
            OpenOrders.AddRange(orders2);
            OpenOrders.Update();

            OnProgress?.Invoke($"Bina({_user.Name}): spot orders done");
        }
    }
}
