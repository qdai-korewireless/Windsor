using System;
using System.Reflection;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CommonServiceLocator.WindsorAdapter;
using Microsoft.Practices.ServiceLocation;

namespace HelloWindsor
{
    class Program
    {
        static void Main(string[] args)
        {

            // application starts...
            var container = new WindsorContainer();
            //register
        
            //Facility
            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<StartableFacility>(f => f.DeferredStart());
            container.Register(Component.For<ISAGCarrierServiceFactory>().AsFactory(c => c.SelectedWith(new CustomTypedFactoryComponentSelector())));
            // adds and configures all components using WindsorInstallers from executing assembly
            container.Install(FromAssembly.InThisApplication());

            var factory = container.Resolve<ISAGCarrierServiceFactory>();
            var req = factory.GetBySimTypeId(6);
            
            req.ProcessTransactions();



            // clean up, application exits
            factory.Release(req);
            container.Dispose();
            Console.Read();
        }
    }

    public interface ISAGCarrierServiceFactory
    {
        ISAGRequest GetBySimTypeId(int simTypeId);
        
        void Release(ISAGRequest request);
    }

    public class RepositoriesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {

            container.Register(Classes.FromAssemblyInThisApplication().Pick()
                .WithService.DefaultInterfaces()
                .LifestyleTransient());

            //container.Register(
            //    Component.For<ISAGRequest>().ImplementedBy<SAGRequest>().Named("SAGRequest"),
            //    Component.For<IATTRequest>().ImplementedBy<ATTRequest>().Named("ATTRequest"));

        }
    }
    public class LoggerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.NLog));

            ServiceLocator.SetLocatorProvider(() =>
                        new WindsorServiceLocator(container));

        }
    }

    public class CustomTypedFactoryComponentSelector : DefaultTypedFactoryComponentSelector
    {
        protected override string GetComponentName(MethodInfo method, object[] arguments)
        {
            if (method.Name == "GetBySimTypeId" && arguments.Length == 1 && arguments[0] is int)
            {
                var simTypeId = (int)arguments[0];
                if (simTypeId == 6) return "HelloWindsor.ATTRequest";
                else throw new Exception("SimTypeId is not found");
            }
            return base.GetComponentName(method, arguments);
        }
    }








    public interface ISAGRequest
    {
        void ProcessTransactions();
        
    }

    public class SAGRequest : ISAGRequest
    {
        protected ILogger _logger;
        public SAGRequest(ILogger logger)
        {
            _logger = logger;
        }

        public void ProcessTransactions()
        {
            _logger.Info("process trans");
            Excute();
            
        }

        public virtual void BeforeExute()
        {
            _logger.Info("BASE - Before Excute SIM");
        }
        public virtual void Excute()
        {
            _logger.Info("BASE - Excute SIM");
        }
        public virtual void AfterExcute()
        {
            _logger.Info("BASE - After Excute SIM");
        }
    }

    public interface IATTRequest : ISAGRequest
    {

    }

    public class ATTRequest : SAGRequest, IATTRequest
    {
        public ATTRequest(ILogger logger) : base(logger)
        {
        }

        public override void BeforeExute()
        {
            _logger.Info("BASE - Before Excute SIM");
        }
        public override void Excute()
        {
            _logger.Info("BASE - Excute SIM");
        }
        public override void AfterExcute()
        {
            _logger.Info("BASE - After Excute SIM");
        }
    }

}
