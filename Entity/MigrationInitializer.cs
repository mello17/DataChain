using System.Data.Entity;
using Entity.Migrations;


namespace DataChain.DataProvider
{
   public class MigrationInitializer : MigrateDatabaseToLatestVersion<DatachainContext, Configuration>
    {
    }
}
