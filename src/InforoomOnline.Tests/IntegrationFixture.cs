using System;
using System.Reflection;
using System.ServiceModel.Description;
using System.Web.Services.Protocols;
using Common.Models;
using Common.Service.Models;
using Common.Service.Tests;
using Common.Tools;
using InforoomOnline.Tests.Properties;
using NHibernate.Expression;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class IntegrationFixture
    {
		private localhost.InforoomOnlineService service;
    	private SessionFactoryHolder sessionFactoryHolder;

		[SetUp]
		public void Setup()
		{
			service = new localhost.InforoomOnlineService();
			SecurityRepositoryFixture.DeleteAllPermissions("kvasov");
			SecurityRepositoryFixture.CreatePermission("kvasov", "IOL");

			sessionFactoryHolder = new SessionFactoryHolder();
			sessionFactoryHolder
				.Configuration
				.Configure()
				.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Service")));
			sessionFactoryHolder.BuildSessionFactory();
		}

    	[Test]
        public void IntegrationTest()
        {
            var offers = service.GetOffers(null, null, false, true, null, null, 1000, true, 0, true);
            Assert.That(offers.Tables.Count,
                        Is.EqualTo(1));
        }

		[Test]
		public void CheckWsdl()
		{
			var metaTransfer =
				new MetadataExchangeClient(new Uri(Settings.Default.InforoomOnline_Tests_localhost_InforoomOnlineService + "?wsdl"),
				                           MetadataExchangeClientMode.HttpGet);
			metaTransfer.ResolveMetadataReferences = true;
			var otherDocs = metaTransfer.GetMetadata();
			Assert.That(otherDocs.MetadataSections.Count, Is.GreaterThan(0));
		}

		[Test]
		public void User_cannot_access_to_service_without_IOL_permission()
		{
			try
			{
				SecurityRepositoryFixture.DeleteAllPermissions("kvasov");
				service.GetPriceList(new[] { "*" });
				Assert.Fail("Должны были завалиться");
			}
			catch (SoapException e)
			{
				Assert.That(e.Message, Is.EqualTo("Нет права доступа"));
			}
		}

		[Test]
		public void User_can_access_to_service_if_have_IOL_Permission()
		{
			service.GetPriceList(new[] { "*" });
		}

		[Test]
		public void Every_message_call_should_be_logged()
		{
			var begin = DateTime.Now;
			using (var session = sessionFactoryHolder.SessionFactory.OpenSession())
			{
				var logs = session.CreateCriteria(typeof(ServiceLogEntity))
					.Add(Expression.Ge("LogTime", new DateTime(begin.Year, begin.Month, begin.Day)))
					.List<ServiceLogEntity>();

				logs.Foreach(session.Delete);
				session.Flush();
			}

			service.GetPriceList(new[] { "*" });

			using (var session = sessionFactoryHolder.SessionFactory.OpenSession())
			{
				var log = session.CreateCriteria(typeof (ServiceLogEntity))
					.Add(Expression.Ge("LogTime", begin))
					.UniqueResult<ServiceLogEntity>();

				Assert.That(log.MethodName, Is.EqualTo("GetPriceList"));
			}
		}
    }
}
