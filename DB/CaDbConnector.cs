using Microsoft.EntityFrameworkCore;

namespace CaOrdersServer
{
    public class CaDbConnector : DbContext
    {
        public static string ConnectionString = "Server=.;Database=CryptoAlert;UID=ca;PWD=1qaz!QAZ";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        public virtual DbSet<CaOrder>? Orders { get; set; }
    }
}
