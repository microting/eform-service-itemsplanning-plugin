/*
The MIT License (MIT)
Copyright (c) 2007 - 2019 Microting A/S
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFormShared;
using ServiceItemsPlanningPlugin.Messages;
using Microsoft.EntityFrameworkCore;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;
using OpenStack.NetCoreSwiftClient.Extensions;
using Rebus.Handlers;

namespace ServiceItemsPlanningPlugin.Handlers
{
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
            string folderId = getFolderId(list.Name).ToString();

            if (siteIds == null || siteIds.Value.IsNullOrEmpty())
            {
                Console.WriteLine("SiteIds not set");
                return;
            }

            Console.WriteLine($"SiteIds {siteIds}");

            foreach (var item in list.Items)
            {
                foreach (var siteIdString in siteIds.Value.Split(','))
                {
                    var siteId = int.Parse(siteIdString);
                    var caseToDelete = await _dbContext.ItemCases.LastOrDefaultAsync(x => x.ItemId == item.Id);
                    
                    if (caseToDelete != null)
                    {
                        _sdkCore.CaseDelete(caseToDelete.MicrotingSdkCaseId.ToString());
                        caseToDelete.WorkflowState = Constants.WorkflowStates.Retracted;
                        await caseToDelete.Update(_dbContext);
                    }

                    mainElement.Label = item.Name;
                    mainElement.ElementList[0].Label = mainElement.Label;
                    mainElement.CheckListFolderName = folderId;
                    
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
