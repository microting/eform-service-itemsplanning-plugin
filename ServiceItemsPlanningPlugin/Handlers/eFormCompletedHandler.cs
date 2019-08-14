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
                var caseDto = _sdkCore.CaseReadByCaseId(message.caseId);
                var microtingUId = caseDto.MicrotingUId;
                var microtingCheckUId = caseDto.CheckUId;
                var theCase = _sdkCore.CaseRead(microtingUId, microtingCheckUId);

                itemCase.MicrotingSdkCaseDoneAt = theCase.DoneAt;
                await itemCase.Update(_dbContext);
            }
        }
    }
}