namespace CaOrdersServer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public enum OState
    {
        Open = 1,
        Filled = 2,
        Canceled = 0,
        NotFound = -1
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
                foreach (CaOrder ord in db.Orders!
                    .Where(x => x.usr_id == usr_id && x.exchange == exchen)
                    .OrderBy(x => x.id)
                )
                {// по всем в базе
                    if (this.Any(o => o.ord_id == ord.ord_id))
                    {
                        ord.dtu = DateTime.Now;
                    }
                    else
                    {// если нет на бирже, то ставим
                        if (ord.dt_create < DateTime.Now.AddDays(-10)) 
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
