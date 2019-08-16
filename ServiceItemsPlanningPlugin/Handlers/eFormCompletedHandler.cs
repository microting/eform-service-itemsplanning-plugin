using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Models;
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
            ItemCase itemCase = await _dbContext.ItemCases.SingleOrDefaultAsync(x => x.MicrotingSdkCaseId == message.caseId);
            
            if (itemCase != null)
            {
                itemCase.Status = 100;
                var caseDto = _sdkCore.CaseReadByCaseId(message.caseId);
                var microtingUId = caseDto.MicrotingUId;
                var microtingCheckUId = caseDto.CheckUId;
                var theCase = _sdkCore.CaseRead(microtingUId, microtingCheckUId);

                itemCase = SetFieldValue(itemCase, theCase.Id);

                itemCase.MicrotingSdkCaseDoneAt = theCase.DoneAt;
                itemCase.DoneByUserId = itemCase.MicrotingSdkSiteId;
                var site = _sdkCore.SiteRead(itemCase.MicrotingSdkSiteId);
                itemCase.DoneByUserName = $"{site.FirstName} {site.LastName}";
                await itemCase.Update(_dbContext);
            }
        }

        private ItemCase SetFieldValue(ItemCase itemCase, int caseId)
        {
            Item item = _dbContext.Items.SingleOrDefault(x => x.Id == itemCase.ItemId);
            ItemList itemList = _dbContext.ItemLists.SingleOrDefault(x => x.Id == item.ItemListId);
            List<int> caseIds = new List<int>();
            caseIds.Add(itemCase.MicrotingSdkCaseId);
            List<FieldValue> fieldValues = _sdkCore.Advanced_FieldValueReadList(caseIds);

            if (itemList == null) return itemCase;

            if (itemList.SdkFieldEnabled1)
            {
                itemCase.SdkFieldValue1 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId1)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled2)
            {
                itemCase.SdkFieldValue2 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId2)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled3)
            {
                itemCase.SdkFieldValue3 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId3)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled4)
            {
                itemCase.SdkFieldValue4 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId4)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled5)
            {
                itemCase.SdkFieldValue5 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId5)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled6)
            {
                itemCase.SdkFieldValue6 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId6)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled7)
            {
                itemCase.SdkFieldValue7 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId7)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled8)
            {
                itemCase.SdkFieldValue8 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId8)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled9)
            {
                itemCase.SdkFieldValue9 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId9)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled10)
            {
                itemCase.SdkFieldValue10 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId10)?.ValueReadable;
            }

            return itemCase;
        }
    }
}