namespace ServiceItemsPlanningPlugin.Scheduler.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Quartz;

    public class SearchListJob : IJob{
    public Task Execute(IJobExecutionContext context)
        {
            try
            {
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
    }
}