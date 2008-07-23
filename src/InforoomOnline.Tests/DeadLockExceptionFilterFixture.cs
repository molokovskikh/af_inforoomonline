using System;
using System.IO;
using System.Reflection;
using System.Text;
using InforoomOnline.Logging;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
	public class FakeAppender : AppenderSkeleton
	{
		public static int InvocationCount;

		protected override void Append(LoggingEvent loggingEvent)
		{
			InvocationCount++;
		}
	}

	[TestFixture]
	public class DeadLockExceptionFilterFixture
	{
		[Test]
		public void Search_dead_lock_exception_in_exception_chaing()
		{
			var filter = new DeadLockExceptionFilter();
			var loggingEvent1205 = new LoggingEvent(typeof(DeadLockExceptionFilterFixture),
													null,
													"root",
													Level.Error,
													"test",
													new Exception("test", CreateException("test", 1205)));
			Assert.That(filter.Decide(loggingEvent1205), Is.EqualTo(FilterDecision.Deny));
		}

		[Test]
		public void Deny_all_dead_lock_exception()
		{
			var filter = new DeadLockExceptionFilter();
			var loggingEvent1205 = new LoggingEvent(typeof (DeadLockExceptionFilterFixture),
			                                        null,
			                                        "root",
			                                        Level.Error,
			                                        "test",
			                                        CreateException("test", 1205));
			Assert.That(filter.Decide(loggingEvent1205), Is.EqualTo(FilterDecision.Deny));

			var loggingEvent1213 = new LoggingEvent(typeof (DeadLockExceptionFilterFixture),
			                                        null,
			                                        "root",
			                                        Level.Error,
			                                        "test",
			                                        CreateException("test", 1213));
			Assert.That(filter.Decide(loggingEvent1213), Is.EqualTo(FilterDecision.Deny));

			var loggingEvent1422 = new LoggingEvent(typeof (DeadLockExceptionFilterFixture),
			                                        null,
			                                        "root",
			                                        Level.Error,
			                                        "test",
			                                        CreateException("test", 1422));
			Assert.That(filter.Decide(loggingEvent1422), Is.EqualTo(FilterDecision.Deny));
		}

		[Test]
		public void Neutral_on_any_event_thar_not_error_and_not_deadlock()
		{
			var filter = new DeadLockExceptionFilter();

			var loggingEvent = new LoggingEvent(typeof (DeadLockExceptionFilterFixture),
			                                    null,
			                                    "root",
			                                    Level.Error,
			                                    "test",
			                                    null);
			Assert.That(filter.Decide(loggingEvent), Is.EqualTo(FilterDecision.Neutral));

			loggingEvent = new LoggingEvent(typeof(DeadLockExceptionFilterFixture),
									null,
									"root",
									Level.Error,
									"test",
									new Exception());
			Assert.That(filter.Decide(loggingEvent), Is.EqualTo(FilterDecision.Neutral));

			loggingEvent = new LoggingEvent(typeof (DeadLockExceptionFilterFixture),
			                                null,
			                                "root",
			                                Level.Info,
			                                "test",
			                                null);
			Assert.That(filter.Decide(loggingEvent), Is.EqualTo(FilterDecision.Neutral));

			loggingEvent = new LoggingEvent(typeof(DeadLockExceptionFilterFixture),
								null,
								"root",
								Level.Info,
								"test",
								CreateException("test", 1422));
			Assert.That(filter.Decide(loggingEvent), Is.EqualTo(FilterDecision.Neutral));
		}

		[Test]
		public void DeadLocakExceptionFilterIntegrationTest()
		{
			FakeAppender.InvocationCount = 0;
			LogManager.ResetConfiguration();
			XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(
@"<?xml version='1.0' encoding='utf-8' ?>
<configuration>

 <configSections>
    <section name='log4net' type='log4net.Config.Log4NetConfigurationSectionHandler,log4net'/>
 </configSections>

  <log4net>
    <appender name='Test' type='InforoomOnline.Tests.FakeAppender, InforoomOnline.Tests'>
	  <filter type='InforoomOnline.Logging.DeadLockExceptionFilter, InforoomOnline' />
      <layout type='log4net.Layout.PatternLayout, log4net'>
        <conversionPattern value='Version: %property{Version}%newlineHost: %property{log4net:HostName} %newlineUserName: %username %newlineDate: %date %newline'/>
      </layout>
    </appender>
    <root>
      <level value='ALL'/>
      <appender-ref ref='Test'/>
    </root>
  </log4net>

</configuration>
".Replace("'", "\""))));

			Assert.That(LogManager.GetRepository().Configured, Is.True, "Не сложилось с конфигурацией");
			var log = LogManager.GetLogger(typeof (DeadLockExceptionFilterFixture));

			log.Error("test", CreateException("test", 1205));
			log.Error("test", CreateException("test", 1213));
			log.Error("test", CreateException("test", 1422));

			Assert.That(FakeAppender.InvocationCount, Is.EqualTo(0), "Вызван аппендер чего не должно быть");

			log.Error("test", new Exception("test"));
			Assert.That(FakeAppender.InvocationCount, Is.EqualTo(1), "Не вызвали аппендер");

			log.Info("test");
			Assert.That(FakeAppender.InvocationCount, Is.EqualTo(2), "Не вызвали аппендер");
		}

		public MySqlException CreateException(string message, int errorCode)
		{
			return (MySqlException) typeof (MySqlException)
			                        	.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
			                        	                null,
			                        	                new[] {typeof (string), typeof (int)},
			                        	                null)
			                        	.Invoke(new object[] {message, errorCode});
		}
	}
}
