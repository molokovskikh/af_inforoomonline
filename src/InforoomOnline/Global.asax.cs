﻿using System;
using System.Reflection;
using System.Web;
using Castle.Core;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common.Models;
using Common.Models.Repositories;
using Common.MySql;
using Common.Service.Interceptors;
using Common.Service.Models;
using log4net;
using log4net.Config;
using NHibernate.Mapping.Attributes;

namespace InforoomOnline
{
	public class Global : HttpApplication
	{
		private readonly ILog _log = LogManager.GetLogger(typeof(Global));

		protected void Application_Start(object sender, EventArgs e)
		{
			try {
				XmlConfigurator.Configure();
				GlobalContext.Properties["Version"] = Assembly.GetExecutingAssembly().GetName().Version;

				ConnectionHelper.DefaultConnectionStringName = "Main";
				var sessionFactoryHolder = new SessionFactoryHolder();
				sessionFactoryHolder
					.Configuration
					.AddInputStream(HbmSerializer.Default.Serialize(typeof(ServiceLogEntity).Assembly))
					.AddInputStream(HbmSerializer.Default.Serialize(typeof(Order).Assembly));

				var container = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(
						Component.For<IInforoomOnlineService>()
							.Named("InforoomOnlineService")
							.ImplementedBy(typeof(InforoomOnlineService))
							.Interceptors(
								InterceptorReference.ForType<ErrorLoggingInterceptor>(),
								InterceptorReference.ForType<LoggingInterceptor>(),
								InterceptorReference.ForType<ContextLoaderInterceptor>(),
								InterceptorReference.ForType<MonitorExecutingTimeInterceptor>(),
								InterceptorReference.ForType<UpdateLastAccessTimeInterceptor>(),
								InterceptorReference.ForType<PermissionCheckInterceptor>())
							.Anywhere,
						Component.For<ErrorLoggingInterceptor>(),
						Component.For<LoggingInterceptor>().Parameters(Parameter.ForKey("ServiceName").Eq("InforoomOnline")),
						Component.For<PermissionCheckInterceptor>().Parameters(Parameter.ForKey("Permission").Eq("IOL")),
						Component.For<UpdateLastAccessTimeInterceptor>().Parameters(Parameter.ForKey("Field").Eq("IOLTime")),
						Component.For<MonitorExecutingTimeInterceptor>(),
						Component.For<ContextLoaderInterceptor>(),
						Component.For<IClientLoader>().ImplementedBy<ClientLoader>(),
						Component.For<LockMonitor>()
							.Parameters(Parameter.ForKey("TimeOut").Eq("10000")),
						Component.For<ISessionFactoryHolder>().Instance(sessionFactoryHolder),
						Component.For<RepositoryInterceptor>(),
						Component.For(typeof(IRepository<>)).ImplementedBy(typeof(Repository<>)),
						Component.For<ISecurityRepository>().ImplementedBy<SecurityRepository>(),
						Component.For<ILogRepository>().ImplementedBy<LogRepository>(),
						Component.For<IOfferRepository>().ImplementedBy<OfferRepository>());
				IoC.Initialize(container);
				IoC.Resolve<LockMonitor>().Start();
			}
			catch (Exception ex) {
				_log.Error("Ошибка при инициализации InforoomOnlineService", ex);
			}
		}

		protected void Application_End(object sender, EventArgs e)
		{
			if (IoC.Container != null) {
				IoC.Resolve<LockMonitor>().Stop();
				IoC.Container.Dispose();
			}
		}
	}
}