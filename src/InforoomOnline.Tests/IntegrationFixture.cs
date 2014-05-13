using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Description;
using System.Web.Services.Protocols;
using Castle.ActiveRecord;
using Common.Models;
using Common.Service.Models;
using Common.Service.Tests;
using Common.Tools;
using InforoomOnline.Tests.Properties;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using Test.Support;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class IntegrationFixture : Test.Support.IntegrationFixture
	{
		private localhost.InforoomOnlineService service;
		private SessionFactoryHolder sessionFactoryHolder;
		private TestUser user;
		private string userName = "DebugUser";

		[SetUp]
		public void Setup()
		{
			service = new localhost.InforoomOnlineService();
			user = session.Query<TestUser>().FirstOrDefault(u => u.Login == userName);
			if (user == null) {
				user = TestClient.CreateNaked(session).Users[0];
				user.Login = "DebugUser";
				session.Save(user);
			}
			else if (user.AssignedPermissions.Count == 0) {
				user.AssignedPermissions.AddEach(session.Query<TestUserPermission>());
				session.Save(user);
			}
			session.Transaction.Commit();
			sessionFactoryHolder = new SessionFactoryHolder();
			sessionFactoryHolder.Configuration.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Service")));
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
			try {
				session.Transaction.Begin();
				user.AssignedPermissions.Clear();
				session.Save(user);
				session.Transaction.Commit();

				service.GetPriceList(new[] { "*" });
				Assert.Fail("Должны были завалиться");
			}
			catch (SoapException e) {
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
			using (var session = sessionFactoryHolder.SessionFactory.OpenSession()) {
				var logs = session.CreateCriteria(typeof(ServiceLogEntity))
					.Add(Expression.Ge("LogTime", new DateTime(begin.Year, begin.Month, begin.Day)))
					.List<ServiceLogEntity>();

				logs.Each(session.Delete);
				session.Flush();
			}

			service.GetPriceList(new[] { "*" });

			using (var session = sessionFactoryHolder.SessionFactory.OpenSession()) {
				var log = session.CreateCriteria(typeof(ServiceLogEntity))
					.Add(Expression.Ge("LogTime", begin))
					.UniqueResult<ServiceLogEntity>();

				Assert.That(log.MethodName, Is.EqualTo("GetPriceList"));
			}
		}

		[Test]
		public void On_every_method_call_last_access_time_should_be_update()
		{
			var begin = DateTime.Now;
			service.GetPriceList(new[] { "*" });

			var lastUpdate = LogRepositoryFixture.GetLastAccessTime(userName, "IOLTime");
			Assert.That(begin - lastUpdate, Is.LessThan(TimeSpan.FromSeconds(10)));
		}
	}
}