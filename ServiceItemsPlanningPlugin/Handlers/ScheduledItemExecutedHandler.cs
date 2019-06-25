namespace ServiceItemsPlanningPlugin.Handlers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microting.ItemsPlanningBase.Infrastructure.Data;
    using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
    using Rebus.Handlers;

    public class ScheduledItemExecutedHandler : IHandleMessages<ScheduledItemExecuted>
    {
        private readonly ItemsPlanningPnDbContext _dbContext;
        private readonly eFormCore.Core _sdkCore;

        public ScheduledItemExecutedHandler(eFormCore.Core sdkCore, ItemsPlanningPnDbContext dbContext)
        {
            _sdkCore = sdkCore;
            _dbContext = dbContext;
        }

        #pragma warning disable 1998
        public async Task Handle(ScheduledItemExecuted message)
        {
            var siteIds = _dbContext.PluginConfigurationValues.FirstOrDefault(x => x.Name == "ItemsPlanningBaseSettings:SiteIds");
            var list = await _dbContext.ItemLists.FindAsync(message.itemListId);
            
            var mainElement = _sdkCore.TemplateRead(list.RelatedEFormId);

            if (siteIds != null)
            {
                foreach (var item in list.Items)
                {
                    foreach (var siteIdString in siteIds.Value.Split(','))
                    {
                        var siteId = int.Parse(siteIdString);
                        var caseToDelete = await _dbContext.ItemCases.LastOrDefaultAsync(x => x.ItemId == item.Id);
                        
                        if (caseToDelete != null)
                        {
                            _sdkCore.CaseDelete(caseToDelete.MicrotingSdkCaseId.ToString());
                        }

                        var caseId = _sdkCore.CaseCreate(mainElement, "", siteId);

                        var itemCase = new ItemCase()
                        {
                            MicrotingSdkSiteId = siteId,
                            MicrotingSdkeFormId = list.RelatedEFormId,
                            Status = 1,
                            MicrotingSdkCaseId = int.Parse(caseId),
                            ItemId = item.Id
                        };

                        await itemCase.Save(_dbContext);
                    } 
                }
            }
        }
    }
}
