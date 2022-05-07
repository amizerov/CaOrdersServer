using Binance.Net.Objects.Models.Spot;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaOrdersServer
{
    public class CaBalance
    {
        public int id { get; set; }
        public int usr_id { get; set; }
        public int exc_id { get; set; }
        public string? Asset { get; set; }
        public decimal Available { get; set; }
        public decimal Locked { get; set; }
        public decimal Total { get; set; }
        public DateTime? dtu { get; set; }

        public CaBalance() { }
        public CaBalance(BinanceBalance b, int uid)
        {
            usr_id = uid;
            exc_id = 1;
            Asset = b.Asset;
            Available = b.Available;
            Locked = b.Locked;
            Total = b.Total;
        }
        public void Update()
        {
            using (CaDbConnector db = new CaDbConnector())
            {
                CaBalance? b = db.Balances!.FirstOrDefault(b => b.usr_id == usr_id && b.Asset == Asset);
                if (b == null)
                {
                    if(Total > 0)
                        db.Balances!.Add(this);
                }
                else
                {
                    b.Available = Available;
                    b.Locked = Locked;
                    b.Total = Total;
                    b.dtu = DateTime.Now;
                }
                db.SaveChanges();
            }
        }
    }
}
