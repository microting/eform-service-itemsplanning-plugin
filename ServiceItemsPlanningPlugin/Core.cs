using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.EntityFrameworkCore;
using Microting.ItemsPlanningBase.Infrastructure.Data;
using Microting.WindowsService.BasePn;
using Rebus.Bus;

namespace ServiceItemsPlanningPlugin
{
    using Installers;
    using Microting.ItemsPlanningBase.Infrastructure.Data.Factories;

    [Export(typeof(ISdkEventHandler))]
    public class Core : ISdkEventHandler
    {
        private eFormCore.Core _sdkCore;
        private IWindsorContainer _container;
        public IBus Bus;
        private bool _coreThreadRunning = false;
        private bool _coreStatChanging;
        private bool _coreAvailable;
        private string _serviceLocation;
        private const int MaxParallelism = 1;
        private const int NumberOfWorkers = 1;
        private ItemsPlanningPnDbContext _dbContext;

        public void CoreEventException(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void UnitActivated(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void eFormProcessed(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void eFormProcessingError(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void eFormRetrived(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void CaseCompleted(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void CaseDeleted(object sender, EventArgs args)
        {
            // Do nothing
        }

        public void NotificationNotFound(object sender, EventArgs args)
        {
            // Do nothing
        }

        public bool Start(string sdkConnectionString, string serviceLocation)
        {
            Console.WriteLine("ServiceItemsPlanningPlugin start called");
            try
            {
                string dbNameSection;
                string dbPrefix;
                if (sdkConnectionString.ToLower().Contains("convert zero datetime"))
                {
                    dbNameSection = Regex.Match(sdkConnectionString, @"(Database=\w*;)").Groups[0].Value;
                    dbPrefix = Regex.Match(sdkConnectionString, @"Database=(\d*)_").Groups[1].Value;
                } else
                {
                    dbNameSection = Regex.Match(sdkConnectionString, @"(Initial Catalog=\w*;)").Groups[0].Value;
                    dbPrefix = Regex.Match(sdkConnectionString, @"Initial Catalog=(\d*)_").Groups[1].Value;
                }
                
                
                var pluginDbName = $"Initial Catalog={dbPrefix}_eform-angular-itemsplanning-plugin;";
                var connectionString = sdkConnectionString.Replace(dbNameSection, pluginDbName);


                if (!_coreAvailable && !_coreStatChanging)
                {
                    _serviceLocation = serviceLocation;
                    _coreStatChanging = true;
                    
                    if (string.IsNullOrEmpty(_serviceLocation))
                        throw new ArgumentException("serviceLocation is not allowed to be null or empty");

                    if (string.IsNullOrEmpty(connectionString))
                        throw new ArgumentException("serverConnectionString is not allowed to be null or empty");

                    ItemsPlanningPnContextFactory contextFactory = new ItemsPlanningPnContextFactory();

                    _dbContext = contextFactory.CreateDbContext(new[] { connectionString });
                    _dbContext.Database.Migrate();

                    _coreAvailable = true;
                    _coreStatChanging = false;

                    startSdkCoreSqlOnly(sdkConnectionString);

                    _container = new WindsorContainer();
                    _container.Register(Component.For<ItemsPlanningPnDbContext>().Instance(_dbContext));
                    _container.Register(Component.For<eFormCore.Core>().Instance(_sdkCore));
                    _container.Install(
                        new RebusHandlerInstaller()
                        , new RebusInstaller(connectionString, MaxParallelism, NumberOfWorkers)
                    );


                    Bus = _container.Resolve<IBus>();
                }
                Console.WriteLine("ServiceItemsPlanningPlugin started");
                return true;
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Start failed " + ex.Message);
                throw;
            }
        }

        public bool Stop(bool shutdownReallyFast)
        {
            try
            {
                if (_coreAvailable && !_coreStatChanging)
                {
                    _coreStatChanging = true;

                    _coreAvailable = false;

                    int tries = 0;
                    while (_coreThreadRunning)
                    {
                        Thread.Sleep(100);
                        Bus.Dispose();
                        tries++;
                    }
                    _sdkCore.Close();

                    _coreStatChanging = false;
                }
            }
            catch (ThreadAbortException)
            {
                //"Even if you handle it, it will be automatically re-thrown by the CLR at the end of the try/catch/finally."
                Thread.ResetAbort(); //This ends the re-throwning
            }

            return true;
        }

        public bool Restart(int sameExceptionCount, int sameExceptionCountMax, bool shutdownReallyFast)
        {
            return true;
        }
        
        public void startSdkCoreSqlOnly(string sdkConnectionString)
        {
            _sdkCore = new eFormCore.Core();

            _sdkCore.StartSqlOnly(sdkConnectionString);
        }
    }
}