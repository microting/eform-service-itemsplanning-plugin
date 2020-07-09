namespace ServiceItemsGroupPlanningPlugin.Handlers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.Helpers;
    using Messages;
    using Microting.ItemsGroupPlanningBase.Infrastructure.Data;
    using Microting.ItemsGroupPlanningBase.Infrastructure.Data.Entities;
    using Rebus.Handlers;

    public class EFormRetrievedHandler : IHandleMessages<eFormRetrieved>
    {
        private readonly eFormCore.Core _sdkCore;
        private readonly ItemsGroupPlanningPnDbContext _dbContext;

        public EFormRetrievedHandler(eFormCore.Core sdkCore, DbContextHelper dbContextHelper)
        {
            _dbContext = dbContextHelper.GetDbContext();
            _sdkCore = sdkCore;
        }

        public async Task Handle(eFormRetrieved message)
        {
            ItemCaseSite itemCaseSite = _dbContext.ItemCaseSites.SingleOrDefault(x => x.MicrotingSdkCaseId == message.caseId);
            if (itemCaseSite != null)
            {
                if (itemCaseSite.Status < 77)
                {
                    itemCaseSite.Status = 77;
                    await itemCaseSite.Update(_dbContext);
                }
            }
        }
    }
}