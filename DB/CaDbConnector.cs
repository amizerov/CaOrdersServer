using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CaOrdersServer
{
    public class CaDbConnector : DbContext
    {
        public static string ConnectionString = "Server=.;Database=CryptoAlert;UID=ca;PWD=1qaz!QAZ";
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.Properties<decimal>().HavePrecision(18, 10);

        public virtual DbSet<CaOrder>? Orders { get; set; }
        public virtual DbSet<CaBalance>? Balances{ get; set; }
    }
}
