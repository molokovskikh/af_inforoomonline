using System;
using System.ServiceModel;
using Castle.Core;
using Castle.Windsor;
using Common.Models;
using InforoomOnline.Logging;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using Rhino.Mocks;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class ErrorLoggingInterceptorFixture
	{
		[Interceptor(typeof(ErrorLoggingInterceptor))]
		public class TestClass
		{
			public virtual void Run()
			{
				throw new FaultException<string>("test", "test");
			}
		}

		[SetUp]
		public void SetUp()
		{
			ServiceContext.GetUserName = () => "kvasov";
			var container = new WindsorContainer();
			IoC.Initialize(container);
			IoC.Container.AddComponent<ErrorLoggingInterceptor>();
			IoC.Container.AddComponent<TestClass>();
		}

		[TearDown]
		public void TearDown()
		{
			IoC.Container.Dispose();
			IoC.Initialize(null);
		}

		[Test]
		public void Do_not_log_fault_exception()
		{
			var testClass = IoC.Resolve<TestClass>();
			var appender = JoinMockedAppender<ErrorLoggingInterceptor>();
			try
			{
				testClass.Run();
			}
			catch (Exception)
			{}
			appender.AssertWasNotCalled(app => app.DoAppend(null),
			                            method => method.IgnoreArguments());
		}

		public static IAppender JoinMockedAppender<T>()
		{
			var appender = MockRepository.GenerateStub<IAppender>();
			appender.Name = "Test appender";
			var logger = (Logger)LogManager.GetLogger(typeof(T)).Logger;
			logger.Level = Level.All;
			logger.Hierarchy.Configured = true;
			logger.AddAppender(appender);
			return appender;
		}
	}
}
