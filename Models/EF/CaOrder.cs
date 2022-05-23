using am.BL;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaOrdersServer
{
    public enum OState
    {
        Canceled,
        Open,
        Filled,
        NotFound,
        Error
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
        public OState state { get; set; }
        public DateTime? dt_create { get; set; }
        public DateTime? dt_exec { get; set; }
        public DateTime? dtu { get; set; }
        public CaOrder() { }

        public bool Update(string src = "*")
        {/* Создает новый или обновляет существующий ордер
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

                    // эти поля ордера измениться не могут
                    o.dt_create = dt_create;
                    o.exchange = exchange;
                    o.spotmar = spotmar; // но могли быть ранее заданы в базе не верно

                    o.symbol = symbol!.ToUpper().Replace("-", "").Replace("/", "");

                    if (msg == "state")
                    {
                        int stt = (int)o.state;
                        G.db_exec($"insert OrderStateHistory(oid, state, src) values({o.id}, {stt}, '{src}')");
                    }
                    if (msg.Length > 0)
                    {
                        o.dtu = DateTime.Now;
                        msg = $"{exchange}({usr_id}): Order({symbol}|{buysel}|{price}) {msg} updated";
                    }
                }
                else
                {
                    db.Orders!.Add(this);
                    newOrUpdated = false;

                    msg = $"{exchange}({usr_id}): New Order({symbol}|{buysel}|{price}) found";
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
