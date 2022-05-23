using am.BL;
using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Socket;
using Huobi.Net.Objects.Models;
using Huobi.Net.Objects.Models.Socket;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects.Models.Spot;
using Kucoin.Net.Objects.Models.Spot.Socket;

namespace CaOrdersServer
{
    public class Order : CaOrder
    {
        public Order() { }
        //---------------------------------
        public Order(BinanceOrder o, bool sm = true /*spot, false - marg*/)
        {
            ord_id = o.Id.ToString();
            exchange = 1; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol;
            spotmar = sm;
            buysel = o.Side == Binance.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.CreateTime;

            state =
                o.Status == Binance.Net.Enums.OrderStatus.New ? OState.Open :
                o.Status == Binance.Net.Enums.OrderStatus.Filled ? OState.Filled :
                o.Status == Binance.Net.Enums.OrderStatus.Canceled ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Rejected ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Expired ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.PartiallyFilled ? OState.Open : OState.Error;

            dt_exec = state == OState.Filled ? o.UpdateTime : null;
        }
        public Order(BinanceStreamOrderUpdate o, int uid, bool sm = true)
        {
            usr_id = uid;

            ord_id = o.Id.ToString();
            exchange = 1; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol;
            spotmar = sm;
            buysel = o.Side == Binance.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.CreateTime;

            state =
                o.Status == Binance.Net.Enums.OrderStatus.New ? OState.Open :
                o.Status == Binance.Net.Enums.OrderStatus.Filled ? OState.Filled :
                o.Status == Binance.Net.Enums.OrderStatus.Canceled ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Rejected ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Expired ? OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.PartiallyFilled ? OState.Open : OState.Error;

            dt_exec = o.Status == Binance.Net.Enums.OrderStatus.Filled ? o.UpdateTime : null;
        }
        //---------------------------------
        public Order(KucoinOrder o, int uid = 0)
        {
            ord_id = o.Id.ToString();
            clientOrd_id = o.ClientOrderId?.ToString();
            exchange = 2; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol;
            spotmar = o.TradeType == TradeType.SpotTrade;
            buysel = o.Side == OrderSide.Buy;
            price = (decimal)o.Price!;
            qty = (decimal)o.Quantity!;
            dt_create = o.CreateTime;

            state = G._B(o.IsActive) ? OState.Open :
                o.Quantity == o.QuantityFilled ? OState.Filled : OState.Canceled;

            // TODO
            dt_exec = state == OState.Filled ? DateTime.Now : null;
        }
        public Order(KucoinStreamOrderBaseUpdate o, int uid)
        {
            usr_id = uid;

            ord_id = o.OrderId;
            exchange = 2;
            symbol = o.Symbol; 
            //spotmar = true; // через Socket не знаем, уточняем через Call
            buysel = o.Side == OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.OrderTime;

            OState stu =
                o.UpdateType == MatchUpdateType.Filled ? OState.Filled :
                o.UpdateType == MatchUpdateType.Canceled ? OState.Canceled :
                o.UpdateType == MatchUpdateType.Open ? OState.Open : OState.NotFound;

            OState sts =
                o.Status == ExtendedOrderStatus.Open ? OState.Open :
                o.Status == ExtendedOrderStatus.Done && stu == OState.Filled ? OState.Filled :
                o.Status == ExtendedOrderStatus.Done && stu == OState.Canceled ? OState.Canceled : OState.NotFound;

            state = sts;

            dt_exec = sts == OState.Filled ? DateTime.Now : null;
        }
        //---------------------------------
        public Order(HuobiOrder o)
        {
            ord_id = o.Id.ToString();
            exchange = 3; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol.ToUpper();
            spotmar = true; // TODO
            buysel = o.Side == Huobi.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.CreateTime;

            state = o.State == Huobi.Net.Enums.OrderState.Created ? OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Filled ? OState.Filled :
                    o.State == Huobi.Net.Enums.OrderState.Submitted ? OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Rejected ? OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.Canceled ? OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.PartiallyFilled ? OState.Open : OState.NotFound;

            dt_exec = state == OState.Filled ? o.CompleteTime : null;
        }
        public Order(HuobiOpenOrder o)
        {
            ord_id = o.Id.ToString();
            exchange = 3;
            symbol = o.Symbol;
            spotmar = true; // TODO
            buysel = o.Side == Huobi.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.CreateTime;
            state = o.State == Huobi.Net.Enums.OrderState.Created ? OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Filled ? OState.Filled :
                    o.State == Huobi.Net.Enums.OrderState.Submitted ? OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Rejected ? OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.Canceled ? OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.PartiallyFilled ? OState.Open : OState.NotFound;
        }
        public Order(HuobiSubmittedOrderUpdate o, int uid)
        {
            usr_id = uid;

            ord_id = o.OrderId.ToString();
            exchange = 3;
            symbol = o.Symbol;
            spotmar = true; // TODO
            buysel = o.Side == Huobi.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = (decimal)o.Quantity!;
            dt_create = o.CreateTime;
            state = o.Status == Huobi.Net.Enums.OrderState.Created ? OState.Open :
                    o.Status == Huobi.Net.Enums.OrderState.Filled ? OState.Filled :
                    o.Status == Huobi.Net.Enums.OrderState.Submitted ? OState.Open :
                    o.Status == Huobi.Net.Enums.OrderState.Rejected ? OState.Canceled :
                    o.Status == Huobi.Net.Enums.OrderState.Canceled ? OState.Canceled :
                    o.Status == Huobi.Net.Enums.OrderState.PartiallyFilled ? OState.Open : OState.NotFound;
        }
    }
    ///////////////////////////////////////////
    public class Orders : List<Order>
    {
        public event Action<string>? OnProgress;

        // Список содержит все ордера юзера, полученные с биржи
        private User _user;
        int exchan = 0;
        public Orders(User usr)
        {
            _user = usr;
        }
        public void Update()
        {
            string listOrdersComaSeparated = "";
            foreach (var o in this)
            {
                listOrdersComaSeparated += "'" + o.ord_id + "',";
            }
            listOrdersComaSeparated += "'0'";
            G.db_exec(
                @$"update Orders set state = 3, dtu = getdate() 
                   where usr_id={_user.ID} and exchange={exchan} 
                    and not ord_id in ({listOrdersComaSeparated})"
            );
        }
        public new void Add(Order o)
        {
            exchan = o.exchange;
            o.usr_id = _user.ID;
            base.Add(o);

            o.Update($"Caller({o.exchange})");

            o.OnProgress += OnProgress;
        }

    }

}
