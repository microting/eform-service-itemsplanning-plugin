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
                var siteIds = _dbContext.PluginConfigurationValues.FirstOrDefault(x => x.Name == "ItemsPlanningBaseSettings:SiteIds");
                var list = await _dbContext.ItemLists.FindAsync(message.itemListId);
                var mainElement = _sdkCore.TemplateRead(list.RelatedEFormId);
                string folderId = getFolderId(list.Name).ToString();
                
                foreach (var siteIdString in siteIds.Value.Split(','))
                {
                    var siteId = int.Parse(siteIdString);
                    var caseToDelete = await _dbContext.ItemCases.LastOrDefaultAsync(x => x.ItemId == item.Id);
                    Case_Dto caseDto = null;
                    
                    if (caseToDelete != null)
                    {
                        caseDto = _sdkCore.CaseLookupCaseId(caseToDelete.MicrotingSdkCaseId);
                        _sdkCore.CaseDelete(caseDto.MicrotingUId);
                        caseToDelete.WorkflowState = Constants.WorkflowStates.Retracted;
                        await caseToDelete.Update(_dbContext);
                    }

                    mainElement.Label = string.IsNullOrEmpty(item.ItemNumber) ? "" : item.ItemNumber;
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.Name}" : $" - {item.Name}";
                    }

                    if (string.IsNullOrEmpty(item.BuildYear))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.BuildYear}" : $" - {item.BuildYear}";
                    }

                    if (string.IsNullOrEmpty(item.Type))
                    {
                        mainElement.Label += string.IsNullOrEmpty(mainElement.Label) ? $"{item.Type}" : $" - {item.Type}";
                    }
                    mainElement.ElementList[0].Label = mainElement.Label;
                    mainElement.CheckListFolderName = folderId;
                    mainElement.StartDate = DateTime.Now.ToUniversalTime();
                    mainElement.EndDate = DateTime.Now.AddYears(10).ToUniversalTime();

                    var caseId = _sdkCore.CaseCreate(mainElement, "", siteId);

                    caseDto = _sdkCore.CaseLookupMUId(caseId);
                    if (caseDto.CaseId != null)
                    {
                        var itemCase = new ItemCase()
                        {
                            MicrotingSdkSiteId = siteId,
                            MicrotingSdkeFormId = list.RelatedEFormId,
                            Status = 66,
                            MicrotingSdkCaseId = (int)caseDto.CaseId,
                            ItemId = item.Id
                        };

                        await itemCase.Create(_dbContext);
                    }
                } 
            }
        }
        
        private int getFolderId(string name)
        {
            List<Folder_Dto> folderDtos = _sdkCore.FolderGetAll(true);

            bool folderAlreadyExist = false;
            int microtingUId = 0;
            foreach (Folder_Dto folderDto in folderDtos)
            {
                if (folderDto.Name == name)
                {
                    folderAlreadyExist = true;
                    microtingUId = (int)folderDto.MicrotingUId;
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
                        microtingUId = (int)folderDto.MicrotingUId;
                    }
                }
            }

            return microtingUId;
        }
    }
}