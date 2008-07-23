using System;
using System.Reflection;
using System.Web;
using Castle.Facilities.WcfIntegration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Common.Models;
using Common.Models.Repositories;
using log4net;
using log4net.Config;
using NHibernate.Mapping.Attributes;

namespace InforoomOnline
{
    public class Global : HttpApplication
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (Global));

        protected void Application_Start(object sender, EventArgs e)
        {
            try
            {
                XmlConfigurator.Configure();
                GlobalContext.Properties["Version"] = Assembly.GetExecutingAssembly().GetName().Version;

                var sessionFactoryHolder = new SessionFactoryHolder();
                sessionFactoryHolder
                    .Configuration
                    .Configure()
                    .AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("InforoomOnline")));
                sessionFactoryHolder.BuildSessionFactory();

                IoC.Initialize(new WindsorContainer(new XmlInterpreter()));
                IoC.Container.Kernel.AddComponentInstance("ISessionFactoryHolder", typeof(ISessionFactoryHolder), sessionFactoryHolder);
                IoC.Container.AddComponent("Repository", typeof(IRepository<>), typeof(Repository<>));
                IoC.Container.AddComponent("RepositoryInterceptor", typeof(RepositoryInterceptor));
                WindsorServiceHostFactory.RegisterContainer(IoC.Container);
            }
            catch (Exception ex)
            {
                _log.Error("Ошибка при инициализации InforoomOnlineService", ex);
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {}

        protected void Application_End(object sender, EventArgs e)
        {}
    }
}
