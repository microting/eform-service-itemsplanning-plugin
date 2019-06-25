namespace ServiceItemsPlanningPlugin.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Jobs;

    public class SchedulerService
    {
        private readonly List<Timer> _timers = new List<Timer>();
        public void ScheduleTask(int hour, int min, double dayInterval, IJob job)
        {
            var now = DateTime.UtcNow;

            var firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0);
            Console.WriteLine($"now is {now.ToString()}");
            Console.WriteLine($"firstRun is {firstRun.ToString()}");
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            var timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new Timer(x =>
            {
                Console.WriteLine("SchedulerService.Time got called");
                job.Execute();
            }, null, timeToGo, TimeSpan.FromDays(dayInterval));
            _timers.Add(timer);
        }
    }
}
