using System.Linq;
using System.Threading.Tasks;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
using Rebus.Handlers;
using ServiceItemsPlanningPlugin.Messages;

namespace ServiceItemsPlanningPlugin.Handlers
{
    public class EFormCompletedHandler : IHandleMessages<eFormCompleted>
    {
        private readonly eFormCore.Core _sdkCore;
        private readonly ItemsPlanningPnDbContext _dbContext;

        public EFormCompletedHandler(eFormCore.Core sdkCore, ItemsPlanningPnDbContext dbContext)
        {
            _dbContext = dbContext;
            _sdkCore = sdkCore;
        }
        
        public async Task Handle(eFormCompleted message)
        {
            ItemCase itemCase = _dbContext.ItemCases.SingleOrDefault(x => x.MicrotingSdkCaseId == message.caseId);
            if (itemCase != null)
            {
                itemCase.Status = 100;
                await itemCase.Update(_dbContext);
            }
        }
    }
}