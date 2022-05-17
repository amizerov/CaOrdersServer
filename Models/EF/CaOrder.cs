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
        public event Action<string>? OnProgress;

        [Key]
        public int id { get; set; }
        public string? ord_id { get; set; }
        public string? clientOrd_id { get; set; }
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

        public bool Update()
        {/* —оздает новый или обновл€ет существующий ордер
          */
            bool newOrUpdated = true;
            using(CaDbConnector db = new CaDbConnector())
            {
                CaOrder? o = db.Orders!.Where(x =>
                                x.usr_id == usr_id &&
                                x.ord_id == ord_id &&
                                x.exchange == exchange).FirstOrDefault();
                string msg = "";
                if (o != null)
                {
                    if (o.qty != qty) msg = "qty"; o.qty = qty;
                    if (o.price != price) msg = "price"; o.price = price;
                    if (o.state != state) msg = "state"; o.state = state;
                    if (o.dt_exec != dt_exec) msg = "dt_exec"; o.dt_exec = dt_exec;

                    o.dt_create = dt_create;
                    o.exchange = exchange;
                    o.spotmar = spotmar;

                    o.symbol = symbol!.ToUpper().Replace("-", "").Replace("/", "");

                    if (msg.Length > 0)
                    {
                        o.dtu = DateTime.Now;
                        msg = $"Order {symbol}|{exchange}|{ord_id} updated";
                    }
                }
                else
                {
                    db.Orders!.Add(this);
                    newOrUpdated = false;

                    msg = $"New Order found {symbol}|{exchange}|{ord_id}";
                }
                if (msg.Length > 0)
                {
                    OnProgress?.Invoke(msg);
                    db.SaveChanges();
                }

                return newOrUpdated;
            }
        }
    }
}
