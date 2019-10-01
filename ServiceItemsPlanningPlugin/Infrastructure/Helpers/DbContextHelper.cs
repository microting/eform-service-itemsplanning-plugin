using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Data.Factories;

namespace ServiceItemsPlanningPlugin.Infrastructure.Helpers
{
    public class DbContextHelper
    {
        private string ConnectionString { get;}

        public DbContextHelper(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ItemsPlanningPnDbContext GetDbContext()
        {
            ItemsPlanningPnContextFactory contextFactory = new ItemsPlanningPnContextFactory();

            return contextFactory.CreateDbContext(new[] { ConnectionString });
        }
    }
}