using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Dto;
using Microting.eForm.Infrastructure.Constants;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
using Rebus.Handlers;
using ServiceItemsPlanningPlugin.Messages;

namespace ServiceItemsPlanningPlugin.Handlers
{
    public class ItemCaseCreateHandler : IHandleMessages<ItemCaseCreate>
    {
        private readonly ItemsPlanningPnDbContext _dbContext;
        private readonly eFormCore.Core _sdkCore;
        
        public ItemCaseCreateHandler(eFormCore.Core sdkCore, ItemsPlanningPnDbContext dbContext)
        {
            _sdkCore = sdkCore;
            _dbContext = dbContext;
        }
        
        public async Task Handle(ItemCaseCreate message)
        {
            Item item = await _dbContext.Items.SingleOrDefaultAsync(x => x.Id == message.itemId);

            if (item != null)
            {
                var siteIds = await _dbContext.PluginConfigurationValues.FirstOrDefaultAsync(x => x.Name == "ItemsPlanningBaseSettings:SiteIds");
                var list = await _dbContext.ItemLists.FindAsync(message.itemListId);
                var mainElement = _sdkCore.TemplateRead(list.RelatedEFormId);
                string folderId = GetFolderId(list.Name).ToString();

                ItemCase itemCase = await _dbContext.ItemCases.SingleOrDefaultAsync(x => x.ItemId == item.Id);

                if (itemCase == null)
                {
                    itemCase = new ItemCase()
                    {
                        ItemId = item.Id,
                        Status = 66,
                        MicrotingSdkeFormId = list.RelatedEFormId
                    };
                    await itemCase.Create(_dbContext);
                }
                
                foreach (var siteIdString in siteIds.Value.Split(','))
                {
                    var siteId = int.Parse(siteIdString);
                    var caseToDelete = await _dbContext.ItemCases.
                        LastOrDefaultAsync(x => x.ItemId == item.Id && x.MicrotingSdkSiteId == siteId);
                    Case_Dto caseDto = null;
                    
                    if (caseToDelete != null)
                    {
                        caseDto = _sdkCore.CaseLookupCaseId(caseToDelete.MicrotingSdkCaseId);
                        if (caseDto.MicrotingUId != null) _sdkCore.CaseDelete((int) caseDto.MicrotingUId);
                        caseToDelete.WorkflowState = Constants.WorkflowStates.Retracted;
                        await caseToDelete.Update(_dbContext);
                    }

                    mainElement.Label = string.IsNullOrEmpty(item.ItemNumber) ? "" : item.ItemNumber;
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.Name}" : $" - {item.Name}";
                    }

                    if (!string.IsNullOrEmpty(item.BuildYear))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.BuildYear}" : $" - {item.BuildYear}";
                    }

                    if (!string.IsNullOrEmpty(item.Type))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.Type}" : $" - {item.Type}";
                    }
                    mainElement.ElementList[0].Label = mainElement.Label;
                    mainElement.CheckListFolderName = folderId;
                    mainElement.StartDate = DateTime.Now.ToUniversalTime();
                    mainElement.EndDate = DateTime.Now.AddYears(10).ToUniversalTime();

                    var caseId = _sdkCore.CaseCreate(mainElement, "", siteId);

                    if (caseId != null) caseDto = _sdkCore.CaseLookupMUId((int) caseId);
                    if (caseDto?.CaseId != null)
                    {
                        var itemCaseSite = new ItemCaseSite()
                        {
                            MicrotingSdkSiteId = siteId,
                            MicrotingSdkeFormId = list.RelatedEFormId,
                            Status = 66,
                            MicrotingSdkCaseId = (int)caseDto.CaseId,
                            ItemId = item.Id,
                            ItemCaseId = itemCase.Id
                        };

                        await itemCaseSite.Create(_dbContext);
                    }
                }
            }
        }
        
        private int GetFolderId(string name)
        {
            List<Folder_Dto> folderDtos = _sdkCore.FolderGetAll(true);

            bool folderAlreadyExist = false;
            int microtingUId = 0;
            foreach (Folder_Dto folderDto in folderDtos)
            {
                if (folderDto.Name == name)
                {
                    folderAlreadyExist = true;
                    if (folderDto.MicrotingUId != null) microtingUId = (int) folderDto.MicrotingUId;
                }
            }

            if (!folderAlreadyExist)
            {
                _sdkCore.FolderCreate(name, "", null);
                folderDtos = _sdkCore.FolderGetAll(true);
                
                foreach (Folder_Dto folderDto in folderDtos)
                {
                    if (folderDto.Name == name)
                    {
                        if (folderDto.MicrotingUId != null) microtingUId = (int) folderDto.MicrotingUId;
                    }
                }
            }

            return microtingUId;
        }
    }
}