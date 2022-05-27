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
    }
}
