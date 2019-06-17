namespace ServiceItemsPlanningPlugin.Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Handlers;
    using Messages;
    using Rebus.Handlers;

    public class RebusHandlerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IHandleMessages<ScheduledItemExecuted>>().ImplementedBy<ScheduledItemExecutedHandler>().LifestyleTransient());
        }
    }
}