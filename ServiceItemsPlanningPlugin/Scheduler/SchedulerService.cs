namespace ServiceItemsPlanningPlugin.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Jobs;

    public class SchedulerService
    {
        private readonly List<Timer> _timers = new List<Timer>();
        public void ScheduleTask(double interval, IJob job)
        {
            var timer = new Timer(x =>
            {
                job.Execute();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(interval));
            _timers.Add(timer);
        }
    }
}
