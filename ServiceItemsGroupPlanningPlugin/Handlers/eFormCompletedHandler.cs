namespace ServiceItemsGroupPlanningPlugin.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.Helpers;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microting.eForm.Dto;
    using Microting.eForm.Infrastructure.Constants;
    using Microting.eForm.Infrastructure.Models;
    using Microting.ItemsPlanningBase.Infrastructure.Data;
    using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
    using Rebus.Handlers;

    public class EFormCompletedHandler : IHandleMessages<eFormCompleted>
    {
        private readonly eFormCore.Core _sdkCore;
        private readonly ItemsPlanningPnDbContext _dbContext;

        public EFormCompletedHandler(eFormCore.Core sdkCore, DbContextHelper dbContextHelper)
        {
            _dbContext = dbContextHelper.GetDbContext();
            _sdkCore = sdkCore;
        }
        
        public async Task Handle(eFormCompleted message)
        {
            ItemCaseSite itemCaseSite = await _dbContext.ItemCaseSites.SingleOrDefaultAsync(x => x.MicrotingSdkCaseId == message.caseId);
            
            if (itemCaseSite != null)
            {
                itemCaseSite.Status = 100;
                var caseDto = _sdkCore.CaseReadByCaseId(message.caseId);
                var microtingUId = caseDto.Result.MicrotingUId;
                var microtingCheckUId = caseDto.Result.CheckUId;
//                if (microtingUId != null && microtingCheckUId != null) {}
                if (microtingUId != null && microtingCheckUId != null)
                {
                    var theCase = _sdkCore.CaseRead((int)microtingUId, (int)microtingCheckUId);

                    itemCaseSite = await SetFieldValue(itemCaseSite, theCase.Id);

                    itemCaseSite.MicrotingSdkCaseDoneAt = theCase.Result.DoneAt;
                    itemCaseSite.DoneByUserId = itemCaseSite.MicrotingSdkSiteId;
                    var site = _sdkCore.SiteRead(itemCaseSite.MicrotingSdkSiteId);
                    itemCaseSite.DoneByUserName = $"{site.Result.FirstName} {site.Result.LastName}";
                    await itemCaseSite.Update(_dbContext);

                    ItemCase itemCase = await _dbContext.ItemCases.SingleOrDefaultAsync(x => x.Id == itemCaseSite.ItemCaseId);
                    if (itemCase.Status != 100)
                    {
                        itemCase.Status = 100;
                        itemCase.MicrotingSdkCaseDoneAt = theCase.Result.DoneAt;
                        itemCase.MicrotingSdkCaseId = itemCaseSite.MicrotingSdkCaseId;
                        itemCase.DoneByUserId = itemCaseSite.MicrotingSdkSiteId;
                        itemCase.DoneByUserName = $"{site.Result.FirstName} {site.Result.LastName}";

                        itemCase = await SetFieldValue(itemCase, theCase.Id);
                        await itemCase.Update(_dbContext);
                    }
                
                    await RetractFromMicroting(itemCase.Id);
                }
            }
        }

        private async Task RetractFromMicroting(int itemCaseId)
        {
            List<ItemCaseSite> itemCaseSites =
                _dbContext.ItemCaseSites.Where(x => x.ItemCaseId == itemCaseId).ToList();
            
            foreach (ItemCaseSite caseSite in itemCaseSites)
            {
                CaseDto caseDto = await _sdkCore.CaseReadByCaseId(caseSite.MicrotingSdkCaseId);
                if (caseDto.MicrotingUId != null) await _sdkCore.CaseDelete((int) caseDto.MicrotingUId);
            }
        }

        private async Task<ItemCaseSite> SetFieldValue(ItemCaseSite itemCaseSite, int caseId)
        {
            Item item = _dbContext.Items.SingleOrDefault(x => x.Id == itemCaseSite.ItemId);
            ItemList itemList = _dbContext.ItemLists.SingleOrDefault(x => x.Id == item.ItemListId);
            List<int> caseIds = new List<int>();
            caseIds.Add(itemCaseSite.MicrotingSdkCaseId);
            List<FieldValue> fieldValues = await _sdkCore.Advanced_FieldValueReadList(caseIds);

            if (itemList == null) return itemCaseSite;

            if (itemList.SdkFieldEnabled1)
            {
                itemCaseSite.SdkFieldValue1 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId1)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled2)
            {
                itemCaseSite.SdkFieldValue2 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId2)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled3)
            {
                itemCaseSite.SdkFieldValue3 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId3)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled4)
            {
                itemCaseSite.SdkFieldValue4 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId4)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled5)
            {
                itemCaseSite.SdkFieldValue5 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId5)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled6)
            {
                itemCaseSite.SdkFieldValue6 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId6)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled7)
            {
                itemCaseSite.SdkFieldValue7 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId7)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled8)
            {
                itemCaseSite.SdkFieldValue8 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId8)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled9)
            {
                itemCaseSite.SdkFieldValue9 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId9)?.ValueReadable;
            }
            if (itemList.SdkFieldEnabled10)
            {
                itemCaseSite.SdkFieldValue10 =
                    fieldValues.SingleOrDefault(x => x.FieldId == itemList.SdkFieldId10)?.ValueReadable;
            }
            if (itemList.NumberOfImagesEnabled)
            {
                itemCaseSite.NumberOfImages = 0;
                foreach (FieldValue fieldValue in fieldValues)
                {
                    if (fieldValue.FieldType == Constants.FieldTypes.Picture)
                    {
                        if (fieldValue.UploadedData != null)
                        {
                            itemCaseSite.NumberOfImages += 1;
                        }
                    }
                }
            }

            return itemCaseSite;
        }

        private async Task<ItemCase> SetFieldValue(ItemCase itemCase, int caseId)
        {
            Item item = _dbContext.Items.SingleOrDefault(x => x.Id == itemCase.ItemId);
            ItemList itemList = _dbContext.ItemLists.SingleOrDefault(x => x.Id == item.ItemListId);
            List<int> caseIds = new List<int>();
            caseIds.Add(itemCase.MicrotingSdkCaseId);
            List<FieldValue> fieldValues = await _sdkCore.Advanced_FieldValueReadList(caseIds);

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
            if (itemList.NumberOfImagesEnabled)
            {
                itemCase.NumberOfImages = 0;
                foreach (FieldValue fieldValue in fieldValues)
                {
                    if (fieldValue.FieldType == Constants.FieldTypes.Picture)
                    {
                        if (fieldValue.UploadedData != null)
                        {
                            itemCase.NumberOfImages += 1;
                        }
                    }
                }
            }

            return itemCase;
        }
    }
}