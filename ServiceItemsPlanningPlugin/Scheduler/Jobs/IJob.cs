namespace ServiceItemsPlanningPlugin.Scheduler.Jobs
{
    using System.Threading.Tasks;

    public interface IJob
    {
        Task Execute();
    }
}