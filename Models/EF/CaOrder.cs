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
                    newOrUpdated = false;

                    OnProgress?.Invoke($"New Order found {symbol}|{exchange}|{ord_id}");
                }
                db.SaveChanges();

                return newOrUpdated;
            }
        }
    }
}
