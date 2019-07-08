using System.Linq;
using System.Threading.Tasks;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
using Rebus.Handlers;
using ServiceItemsPlanningPlugin.Messages;

namespace ServiceItemsPlanningPlugin.Handlers
{
    public class EFormRetrievedHandler : IHandleMessages<eFormRetrieved>
    {
        private readonly eFormCore.Core _sdkCore;
        private readonly ItemsPlanningPnDbContext _dbContext;

        public EFormRetrievedHandler(eFormCore.Core sdkCore, ItemsPlanningPnDbContext dbContext)
        {
            _dbContext = dbContext;
            _sdkCore = sdkCore;
        }

        public async Task Handle(eFormRetrieved message)
        {
            ItemCase itemCase = _dbContext.ItemCases.SingleOrDefault(x => x.MicrotingSdkCaseId == int.Parse(message.caseId));
            if (itemCase != null)
            {
                if (itemCase.Status < 77)
                {
                    itemCase.Status = 77;
                    await itemCase.Update(_dbContext);
                }
            }
        }
    }
}