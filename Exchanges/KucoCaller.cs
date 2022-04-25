using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
    class KucoCaller
    {
        public event Action<string>? OnProgress;
        
        User _user;
        string? _key, _sec, _pas;

        public KucoCaller(User usr)
        {
            _user = usr;
            ApiKey? keys = _user.ApiKeys.Find(k => k.Exchange == "Kuco");
            if (keys == null) return;

            _key = keys.Key;
            _sec = keys.Secret;
            _pas = keys.PassPhrase;
        }
        public bool CheckApiKey()
        {
            bool b = false; if (_key == null) return b;
            KucoinClient client = new KucoinClient(
                new KucoinClientOptions()
                {
                    ApiCredentials = new KucoinApiCredentials(_key!, _sec!, _pas!)
                });

            var a = client.SpotApi.Account;
            var r = a.GetDepositsAsync().Result;

            if (r!= null && r.Success)
                foreach(KucoinDeposit d in r.Data.Items)
                {
                    if(d.Quantity > 0)
                        b = true;
                }  

            return b;
        }
        public void CheckOrdersSpot()
        {
            if (_key == null) return;
            OnProgress?.Invoke($"Kuco({_user.Name}): check spot orders start");

            KucoinClient client = new KucoinClient(
                new KucoinClientOptions()
                {
                    ApiCredentials = new KucoinApiCredentials(_key!, _sec!, _pas!)
                });

            CaOrders orders = new CaOrders(_user.ID, 2/*Kuco*/);
            var ra = client.SpotApi.Trading.GetOrdersAsync(
                null, null, null, null, null, OrderStatus.Active).Result;
            if (ra.Success)
            {
                foreach (KucoinOrder o in ra.Data.Items)
                {
                    CaOrder order = new CaOrder();
                    order.usr_id = _user.ID;

                    order.ord_id = o.Id;
                    order.exchange = 2; // 1 - Bina, 2 - Kuco, 3 - Huob
                    order.symbol = o.Symbol.ToUpper();
                    order.spotmar = true;
                    order.buysel = o.Side == OrderSide.Buy;
                    order.price = (decimal)o.Price!;
                    order.qty = (decimal)o.Quantity!;
                    order.dt_create = o.CreateTime;
                    order.state = (int)OState.Open;

                    order.Save();
                    orders.Add(order);
                
                    OnProgress?.Invoke($"Kuco({_user.Name}): Opened spot order {o.Id} - {o.Symbol} - {o.Price}");
                }
            }
            var rd = client.SpotApi.Trading.GetOrdersAsync(
                null, null, null, null, null, OrderStatus.Done).Result;
            if (rd.Success)
            {
                foreach (KucoinOrder o in rd.Data.Items)
                {
                    CaOrder order = new CaOrder();
                    order.usr_id = _user.ID;

                    order.ord_id = o.Id;
                    order.exchange = 2; // 1 - Bina, 2 - Kuco, 3 - Huob
                    order.symbol = o.Symbol.ToUpper();
                    order.spotmar = true;
                    order.buysel = o.Side == OrderSide.Buy;
                    order.price = (decimal)o.Price!;
                    order.qty = (decimal)o.Quantity!;
                    order.dt_create = o.CreateTime;
                    order.state = (int)OState.Filled;

                    if (order.state == 2)
                        order.dt_exec = DateTime.Now;

                    order.Save();
                    orders.Add(order);
                
                    OnProgress?.Invoke($"Kuco({_user.Name}): Order {o.Id} - {o.Symbol} - state = Filled");
                }
            }
            orders.Update(); // TODO: не все статусы обработаны
        
            OnProgress?.Invoke($"Kuco({_user.Name}): spot orders done");
        }
    }
}
