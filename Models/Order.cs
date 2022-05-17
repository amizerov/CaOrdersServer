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
                o.Status == Binance.Net.Enums.OrderStatus.New ? (int)OState.Open :
                o.Status == Binance.Net.Enums.OrderStatus.Filled ? (int)OState.Filled :
                o.Status == Binance.Net.Enums.OrderStatus.Canceled ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Rejected ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Expired ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.Error;
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
                o.Status == Binance.Net.Enums.OrderStatus.New ? (int)OState.Open :
                o.Status == Binance.Net.Enums.OrderStatus.Filled ? (int)OState.Filled :
                o.Status == Binance.Net.Enums.OrderStatus.Canceled ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Rejected ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.Expired ? (int)OState.Canceled :
                o.Status == Binance.Net.Enums.OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.Error;

            if (o.Status == Binance.Net.Enums.OrderStatus.Filled) dt_exec = o.UpdateTime;
        }
        //---------------------------------
        public Order(KucoinOrder o)
        {
            ord_id = o.Id.ToString();
            clientOrd_id = o.ClientOrderId?.ToString();
            exchange = 2; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol;
            spotmar = o.TradeType == TradeType.SpotTrade;
            buysel = o.Side == Kucoin.Net.Enums.OrderSide.Buy;
            price = (decimal)o.Price!;
            qty = (decimal)o.Quantity!;
            dt_create = o.CreateTime;
            state =
                (bool)o.IsActive! ? (int)OState.Open : (int)OState.Filled;
        }
        public Order(KucoinStreamOrderBaseUpdate o, int uid)
        {
            usr_id = uid;

            ord_id = o.OrderId;
            exchange = 2;
            symbol = o.Symbol;
            spotmar = true; // TODO
            buysel = o.Side == Kucoin.Net.Enums.OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.OrderTime;
            state =
                o.Status == Kucoin.Net.Enums.ExtendedOrderStatus.Open ? (int)OState.Open :
                o.Status == Kucoin.Net.Enums.ExtendedOrderStatus.Done ? (int)OState.Filled :
                o.UpdateType == Kucoin.Net.Enums.MatchUpdateType.Canceled ? (int)OState.Canceled : (int)OState.Open;
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
            state = o.State == Huobi.Net.Enums.OrderState.Created ? (int)OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Filled ? (int)OState.Filled :
                    o.State == Huobi.Net.Enums.OrderState.Submitted ? (int)OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Rejected ? (int)OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.Canceled ? (int)OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.PartiallyFilled ? (int)OState.Open : (int)OState.NotFound;
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
            state = o.State == Huobi.Net.Enums.OrderState.Created ? (int)OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Filled ? (int)OState.Filled :
                    o.State == Huobi.Net.Enums.OrderState.Submitted ? (int)OState.Open :
                    o.State == Huobi.Net.Enums.OrderState.Rejected ? (int)OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.Canceled ? (int)OState.Canceled :
                    o.State == Huobi.Net.Enums.OrderState.PartiallyFilled ? (int)OState.Open : (int)OState.NotFound;
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
            state = o.Status == Huobi.Net.Enums.OrderState.Created ? (int)OState.Open :
                    o.Status == Huobi.Net.Enums.OrderState.Filled ? (int)OState.Filled :
                    o.Status == Huobi.Net.Enums.OrderState.Submitted ? (int)OState.Open :
                    o.Status == Huobi.Net.Enums.OrderState.Rejected ? (int)OState.Canceled :
                    o.Status == Huobi.Net.Enums.OrderState.Canceled ? (int)OState.Canceled :
                    o.Status == Huobi.Net.Enums.OrderState.PartiallyFilled ? (int)OState.Open : (int)OState.NotFound;
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
            listOrdersComaSeparated += 0;
            G.db_exec(
                @$"update Orders set state = -1, dtu = getdate() 
                   where usr_id={_user.ID} and exchange={exchan} 
                    and not ord_id in ({listOrdersComaSeparated})"
            );
        }
        public new void Add(Order o)
        {
            exchan = o.exchange;
            o.usr_id = _user.ID;
            base.Add(o);

            o.Update();

            o.OnProgress += OnProgress;
        }

    }

}
