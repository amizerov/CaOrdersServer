using am.BL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data;

namespace CaOrdersServer
{
    public class CaDbConnector : DbContext
    {
        public static string ConnectionString = "Server=.;Database=CryptoAlert;UID=ca;PWD=1qaz!QAZ";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.Properties<decimal>().HavePrecision(28, 9);

        public virtual DbSet<CaOrder>? Orders { get; set; }
        public virtual DbSet<CaBalance>? Balances{ get; set; }
    }
    public class Db
    {
        public static DataTable GetOrdersDt(int uid, int eid)
        {
            string where = uid > 0 ? " where usr_id=" + uid : "";
            where += uid > 0 && eid > 0 ? " and exchange=" + eid : "";

            string sql = $@"
                select 
                    case exchange when 1 then 'Bina' when 2 then 'Kuco' else 'Huob' end Exchange,
                    case state when 0 then 'Canc' when 1 then 'Open' when 2 then 'Fill' else 'Not Found' end Status,
                    *
                from Orders o join Users u on o.usr_id=u.id
            ";

            DataTable dt = G.db_select(sql + where);
            return dt;
        }
    }
}
