namespace ServiceItemsPlanningPlugin.Scheduler
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;
    using Quartz.Impl;

    public class QuartzStartup
    {
        private IScheduler _scheduler;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QuartzStartup(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Action Start(Action<QuartzStartup> registerJobs)
        {
            StartAsync().Wait();
            return () => { registerJobs.Invoke(this); };
        }

        public async Task StartAsync()
        {
            if (_scheduler != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = await schedulerFactory.GetScheduler();
            _scheduler.JobFactory = new QuartzJobFactory(_serviceScopeFactory);
            await _scheduler.Start();
        }

        public void AddJob<T>(string name, string group, TimeSpan interval)
            where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .WithIdentity(name, group)
                .Build();

            var jobTrigger = TriggerBuilder.Create()
                .WithIdentity(name + "Trigger", group)
                .StartNow()
                .WithSimpleSchedule(t =>
                    t.WithInterval(interval).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, jobTrigger).Wait();
        }

        public void Stop()
        {
            if (_scheduler == null)
            {
                return;
            }

            // give running jobs 30 sec (for example) to stop gracefully
            if (_scheduler.Shutdown(true).Wait(30000))
            {
                _scheduler = null;
            }
        }
    }
}