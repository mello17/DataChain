using System.Data.Entity;
using Entity.Migrations;


namespace DataChain.EntityFramework
{
   public class MigrationInitializer : MigrateDatabaseToLatestVersion<DatachainContext, Configuration>
    {
    }
}
