using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaOrdersServer
{
    public enum OState
    {
        Open = 1,
        Filled = 2,
        Canceled = 0,
        NotFound = -1,
        Error = -2
    }

    [Table("Orders")]
    public class CaOrder
    {
        [Key]
        public int id { get; set; }
        public string? ord_id { get; set; }
        public int usr_id { get; set; }
        public string? symbol { get; set; }
        public int exchange { get; set; }
        public bool spotmar { get; set; }
        public bool buysel { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public int state { get; set; }
        public DateTime? dt_create { get; set; }
        public DateTime? dt_exec { get; set; }
        public DateTime? dtu { get; set; }
        public CaOrder() { }
        public CaOrder(BinanceOrder o, int uid, bool sm = true /*spot, false - marg*/)
        {
            usr_id = uid;

            ord_id = o.Id.ToString();
            exchange = 1; // 1 - Bina, 2 - Kuco, 3 - Huob
            symbol = o.Symbol;
            spotmar = sm;
            buysel = o.Side == OrderSide.Buy;
            price = o.Price;
            qty = o.Quantity;
            dt_create = o.CreateTime;
            state = 
                o.Status == OrderStatus.New ? (int)OState.Open :
                o.Status == OrderStatus.Filled ? (int)OState.Filled :
                o.Status == OrderStatus.Canceled ? (int)OState.Canceled :
                o.Status == OrderStatus.Rejected ? (int)OState.Canceled :
                o.Status == OrderStatus.Expired ? (int)OState.Canceled :
                o.Status == OrderStatus.PartiallyFilled ? (int)OState.Open : (int)OState.Error;

            if (o.Status == OrderStatus.Filled) dt_exec = o.UpdateTime;
        }
        public void Save()
        {
            using(CaDbConnector db = new CaDbConnector())
            {
                CaOrder? o = db.Orders!.Where(x =>
                                x.usr_id == usr_id &&
                                x.ord_id == ord_id &&
                                x.exchange == exchange).FirstOrDefault();
                if (o != null)
                {
                    o.qty = qty;
                    o.price = price;
                    o.state = state;
                    o.spotmar = spotmar;
                    o.dt_exec = dt_exec;
                    o.dt_create = dt_create;
                    o.exchange = exchange;
                    o.symbol = symbol!.ToUpper().Replace("-", "").Replace("/", "");
                    o.dtu = DateTime.Now;
                }
                else
                {
                    db.Orders!.Add(this);
                }
                db.SaveChanges();
            }
        }
    }
    public class CaOrders : List<CaOrder>
    {
        // Список содержит все ордера юзера, полученные с биржи
        int usr_id = 0;
        int exchen = 0;
        public CaOrders(int uid, int exc)
        {
            usr_id = uid; exchen = exc;
        }
        public void Update()
        {
            using (CaDbConnector db = new CaDbConnector())
            {
                // посмотреть ордера юзера в базе и пометить удаленными если нет на бирже
                List<CaOrder> os = db.Orders!
                    .Where(x => x.usr_id == usr_id && x.exchange == exchen).ToList();

                foreach (CaOrder ord in os)
                {// по всем в базе
                    if (this.Any(o => o.ord_id == ord.ord_id))
                    {
                        ord.dtu = DateTime.Now;
                    }
                    else
                    {// если нет на бирже, то ставим
                        if (ord.dt_create < DateTime.Now.AddDays(-120)) 
                        {
                            ord.state = (int)OState.NotFound; // удален или отменен, нет на бирже
                            ord.dtu = DateTime.Now;
                        }
                    }
                }
                db.SaveChanges();
            }
        }
    }
}
