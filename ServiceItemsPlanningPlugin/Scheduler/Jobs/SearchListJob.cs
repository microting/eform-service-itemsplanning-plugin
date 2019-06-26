﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ServiceItemsPlanningPlugin.Extensions;
using ServiceItemsPlanningPlugin.Messages;
using Microsoft.EntityFrameworkCore;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.ItemsPlanningBase.Infrastructure.Enums;
using Rebus.Bus;

namespace ServiceItemsPlanningPlugin.Scheduler.Jobs
{
    using System.Collections.Generic;
    using Microting.ItemsPlanningBase.Infrastructure.Data.Entities;

    public class SearchListJob : IJob
    {
        private readonly ItemsPlanningPnDbContext _dbContext;
        private readonly IBus _bus;

        public SearchListJob(ItemsPlanningPnDbContext dbContext, IBus bus)
        {
            _dbContext = dbContext;
            _bus = bus;
        }

        public async Task Execute()
        {
            Console.WriteLine("SearchListJob.Execute got called");
            var now = DateTime.UtcNow;
            var lastDayOfMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1).Day;


            var baseQuery = _dbContext.ItemLists.Where(x => x.RepeatUntil == null || DateTime.UtcNow <= x.RepeatUntil);

            var dailyListsQuery = baseQuery
                .Where(x => x.RepeatType == RepeatType.Day 
                            && (x.LastExecutedTime == null || 
                                now.AddDays(-x.RepeatEvery) >= x.LastExecutedTime));

            var weeklyListsQuery = baseQuery
                .Where(x => x.RepeatType == RepeatType.Week 
                            && (x.LastExecutedTime == null || 
                                (now.AddDays(-x.RepeatEvery * 7) >= x.LastExecutedTime && x.DayOfWeek == now.DayOfWeek)));

            var monthlyListsQuery = baseQuery
                .Where(x => x.RepeatType == RepeatType.Month 
                            && (x.LastExecutedTime == null || 
                                ((x.DayOfMonth <= now.Day || now.Day == lastDayOfMonth) && 
                                 (now.AddMonths(-x.RepeatEvery).Month >= x.LastExecutedTime.Value.Month 
                                  || now.AddMonths(-x.RepeatEvery).Year > x.LastExecutedTime.Value.Year))));
            
            Console.WriteLine($"Daily lists query: {dailyListsQuery.ToSql()}");
            Console.WriteLine($"Weekly lists query: {weeklyListsQuery.ToSql()}");
            Console.WriteLine($"Monthly lists query: {monthlyListsQuery.ToSql()}");
            
            var dailyLists = await dailyListsQuery.ToListAsync();
            var weeklyLists = await weeklyListsQuery.ToListAsync();
            var monthlyLists = await monthlyListsQuery.ToListAsync();

            Console.WriteLine($"Found {dailyLists.Count} daily lists");
            Console.WriteLine($"Found {weeklyLists.Count} weekly lists");
            Console.WriteLine($"Found {monthlyLists.Count} monthly lists");

            var scheduledItemLists = new List<ItemList>();
            scheduledItemLists.AddRange(dailyLists);
            scheduledItemLists.AddRange(weeklyLists);
            scheduledItemLists.AddRange(monthlyLists);

            foreach (var list in scheduledItemLists)
            {
                list.LastExecutedTime = now;
                await list.Update(_dbContext);

                await _bus.SendLocal(new ScheduledItemExecuted(list.Id));

                Console.WriteLine($"List {list.Name} executed");
            }
        }
    }
}