using CryptoExchange.Net.Authentication;
using Huobi.Net.Clients;
using Huobi.Net.Objects;
using Huobi.Net.Enums;

namespace CaOrdersServer
{
    public class HuobCaller
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
            bool b = false;
            if (String.IsNullOrEmpty(_key) || String.IsNullOrEmpty(_sec)) return b;

            HuobiClient client = new HuobiClient(
                new HuobiClientOptions
                {
                    ApiCredentials = new ApiCredentials(_key!, _sec!)
                });
            var r = client.SpotApi.Account.GetAccountsAsync().Result;
            if (r != null && r.Success)
            {
                b = true;
            }
            return b;
        }
        public void CheckOrdersSpot()
        {
            if (_key == null) return;
            OnProgress?.Invoke($"Huob({_user.Name}): check spot orders start");

            HuobiClient client = new HuobiClient(
                new HuobiClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_key!, _sec!)
                });

           CaOrders orders = new CaOrders(_user.ID, 3/*Huob*/);
           var ro = client.SpotApi.Trading.GetOpenOrdersAsync().Result;
            if (ro.Success)
            {
                foreach (var o in ro.Data)
                {
                    CaOrder order = new CaOrder();
                    order.usr_id = _user.ID;

                    order.ord_id = o.Id.ToString();
                    order.exchange = 3; // 1 - Bina, 2 - Kuco, 3 - Huob
                    order.symbol = o.Symbol.ToUpper();
                    order.spotmar = true;
                    order.buysel = o.Side == OrderSide.Buy;
                    order.price = o.Price;
                    order.qty = o.Quantity;
                    order.dt_create = o.CreateTime;
                    order.state = (int)OState.Open;

                    if (o.State == OrderState.Filled)
                        order.dt_exec = o.CompleteTime;

                    order.Save();
                    orders.Add(order);

                    OnProgress?.Invoke($"Huob({_user.Name}): Opened spot order {o.Id} - {o.Symbol} - {o.Price}");
                }
            }
            var r = client.SpotApi.Trading.GetHistoricalOrdersAsync().Result;
            if (r.Success)
            {
                foreach (var o in r.Data)
                {
                    CaOrder order = new CaOrder();
                    order.usr_id = _user.ID;

                    order.ord_id = o.Id.ToString();
                    order.exchange = 3; // 1 - Bina, 2 - Kuco, 3 - Huob
                    order.symbol = o.Symbol;
                    order.spotmar = true;
                    order.buysel = o.Side == OrderSide.Buy;
                    order.price = o.Price;
                    order.qty = o.Quantity;
                    order.dt_create = o.CreateTime;
                    order.state = o.State == OrderState.Created ? (int)OState.Open :
                                    o.State == OrderState.Filled ? (int)OState.Filled :
                                    o.State == OrderState.Submitted ? (int)OState.Open :
                                    o.State == OrderState.Rejected ? (int)OState.Canceled :
                                    o.State == OrderState.Canceled ? (int)OState.Canceled :
                                    o.State == OrderState.PartiallyFilled ? (int)OState.Open : (int)OState.NotFound;

                    if (o.State == OrderState.Filled)
                        order.dt_exec = o.CompleteTime;

                    order.Save();
                    orders.Add(order);

                    OnProgress?.Invoke($"Huob({_user.Name}): Order {o.Id} - {o.Symbol} - state = {o.State}");
                }
            }
            orders.Update();

            OnProgress?.Invoke($"Huob({_user.Name}): spot orders done");
        }
    }
}
