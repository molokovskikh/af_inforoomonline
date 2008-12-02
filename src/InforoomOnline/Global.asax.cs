using System;
using System.Reflection;
using System.Web;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common.Models;
using Common.Models.Repositories;
using Common.Service.Interceptors;
using Common.Service.Models;
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
                    .AddInputStream(HbmSerializer.Default.Serialize(typeof(ServiceLogEntity).Assembly));
                sessionFactoryHolder.BuildSessionFactory();

            	var container = new WindsorContainer()
            		.AddFacility<WcfFacility>()
            		.Register(
						Component.For<IInforoomOnlineService>()
						.Named("InforoomOnlineService")
						.ImplementedBy(typeof(InforoomOnlineService)),
						Component.For<ErrorLoggingInterceptor>(),
						Component.For<ResultLogingInterceptor>(),
						Component.For<PermissionCheckInterceptor>(),
						Component.For<UpdateLastAccessTimeInterceptor>(),
						Component.For<ISessionFactoryHolder>().Instance(sessionFactoryHolder),
						Component.For<RepositoryInterceptor>(),
						Component.For(typeof(IRepository<>)).ImplementedBy(typeof(Repository<>)),
						Component.For<ISecurityRepository>().ImplementedBy<SecurityRepository>(),
						Component.For<ILogRepository>().ImplementedBy<LogRepository>()
					);
                IoC.Initialize(container);
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
