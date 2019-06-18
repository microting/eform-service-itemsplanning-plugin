namespace ServiceItemsPlanningPlugin.Scheduler.Factories
{
    using Castle.Windsor;
    using Quartz;
    using Quartz.Spi;

    public class QuartzJobFactory : IJobFactory
    {

        private readonly IWindsorContainer _container;

        public QuartzJobFactory(IWindsorContainer container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _container.Resolve(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            // we let the DI container handler this
        }
    }
}