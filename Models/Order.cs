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
        public User? User;
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

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
            usr_id = uid;

            ord_id = o.Id.ToString();
            clientOrd_id = o.ClientOrderId?.ToString();
            exchange = (int)Exch.Kuco; // 1 - Bina, 2 - Kuco, 3 - Huob
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
            exchange = (int)Exch.Kuco;
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
        //---------------------------------
        public bool Update(string src = "*")
        {/* Создает новый или обновляет существующий ордер
          */
            bool newOrUpdated = true;
            using (CaDbConnector db = new CaDbConnector())
            {
                try
                {
                    CaOrder? o = db.Orders!.Where(x =>
                                    x.usr_id == usr_id &&
                                    x.symbol == symbol &&
                                    x.ord_id == ord_id &&
                                    x.exchange == exchange).FirstOrDefault();
                    string msg = "";
                    if (o != null)
                    {
                        if (o.state != state)
                        {
                            msg = $"state ({o.state}|{state})";
                        }
                        o.state = state;
                        if (o.dt_exec is null && state == OState.Filled)
                        {
                            msg += "|dt_exec";
                            o.dt_exec = dt_exec;
                        }

                        // эти поля ордера измениться не могут
                        o.spotmar = spotmar; // но могли быть ранее заданы в базе не верно

                        if (msg.Contains("state"))
                        {
                            int stt = (int)o.state;
                            G.db_exec($"insert OrderStateHistory(oid, state, src) values({o.id}, {stt}, '{src}')");
                            Progress(new Message(3, this.User!, (Exch)exchange, "Order.Update", msg));
                        }
                        if (msg.Length > 0)
                        {
                            o.dtu = DateTime.Now;
                            msg = $"Order({symbol}|{(buysel ? "Buy" : "Sel")}|{price}) {msg} updated";
                        }
                        newOrUpdated = false;
                    }
                    else
                    {
                        db.Orders!.Add(this);
                        newOrUpdated = true;

                        msg = $"New Order({ord_id}|{symbol}|{(buysel ? "Buy" : "Sel")}|{price}|{qty}) found {(OState)state}";
                    }
                    if (msg.Length > 0)
                    {
                        Progress(new Message(3, this.User!, (Exch)exchange, "Order.Update", msg));
                        try
                        {
                            dtu = DateTime.Now;
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            msg = msg + "\r\n" + "Exception: " + ex.Message + "\r\n" + ex.InnerException;
                            Progress(new Message(3, this.User!, (Exch)exchange, "Order.Update", msg));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Progress(new Message(3, this.User!, (Exch)exchange, "Exception in Order.Update", ex.Message));
                }
                return newOrUpdated;
            }
        }

    }
    ///////////////////////////////////////////
    public class Orders : List<Order>
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        // Список содержит все ордера юзера, полученные с биржи
        private User _user;
        int exchan = 0;
        public Orders(User usr)
        {
            _user = usr;
        }
        public void Update()
        {
            int cnt = this.Count;
            int opn = this.Where(o => o.state == OState.Open).Count();
            int can = this.Where(o => o.state == OState.Canceled).Count();
            int fil = this.Where(o => o.state == OState.Filled).Count();

            string listOrdersComaSeparated = "";
            foreach (var o in this)
            {
                listOrdersComaSeparated += "'" + o.ord_id + "',";
            }
            listOrdersComaSeparated += "'0'";

            int not = G._I(G.db_select(
                @$"
                    update Orders set IsNotFound=1, dtu = getdate() 
                     where usr_id={_user.ID} and exchange={exchan} 
                       and not ord_id in ({listOrdersComaSeparated})
                
                    select @@ROWCOUNT
                "
            ));

            DateTime f = (DateTime)this.MinBy(o => o.dt_create)?.dt_create!;
            DateTime t = (DateTime)this.MaxBy(o => o.dt_create)?.dt_create!;

            int ind = G._I(G.db_select(
                @$"
                    select count(*) c from Orders 
                    where usr_id={_user.ID} and exchange={exchan} 
                    and dt_create between '{f.ToString("yyyy-MM-dd")} 00:00' and '{t.ToString("yyyy-MM-dd")} 23:59'
                "
            ));

            Progress(new Message(3, _user, (Exch)exchan, "Orders.Update", 
                $"all:{cnt} opn:{opn} can:{can} fil:{fil} not:{not} ind:{ind} from:{f} to:{t}"));
        }
        public new void Add(Order o)
        {
            o.User = _user;
            o.usr_id = _user.ID;
            exchan = o.exchange;

            if (!this.Any(ord => ord.ord_id == o.ord_id))
            {
                base.Add(o);
                o.OnProgress += Progress;
                o.Update($"{(Exch)o.exchange}Caller");
            }
            else
            {
                Progress(new Message(3, _user, (Exch)exchan, 
                    "Order.Add", $"this order({o.ord_id}|{o.dt_create}) already added"));
            }
        }
    }
}
