using System.Data.Entity;


namespace DataChain.EntityFramework
{
    public class DatachainContext : DbContext
    {
        public DbSet<BlockModel> Blocks { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<RecordModel> Records { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }

        public DatachainContext(string connString) : base(connString)
        {
            
        }

        public DatachainContext()
        {

        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
    }
}
