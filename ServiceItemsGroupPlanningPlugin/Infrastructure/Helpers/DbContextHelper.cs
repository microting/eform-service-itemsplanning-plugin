namespace ServiceItemsGroupPlanningPlugin.Infrastructure.Helpers
{
    using Microting.ItemsGroupPlanningBase.Infrastructure.Data;
    using Microting.ItemsGroupPlanningBase.Infrastructure.Data.Factories;

    public class DbContextHelper
    {
        private string ConnectionString { get;}

        public DbContextHelper(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ItemsGroupPlanningPnDbContext GetDbContext()
        {
            ItemsGroupPlanningPnContextFactory contextFactory = new ItemsGroupPlanningPnContextFactory();

            return contextFactory.CreateDbContext(new[] { ConnectionString });
        }
    }
}