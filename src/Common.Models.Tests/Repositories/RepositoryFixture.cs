using System;
using System.Reflection;
using Castle.Windsor;
using Common.Models.Repositories;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Models.Tests.Repositories
{
	[TestFixture]
	public class RepositoryFixture
	{
		[SetUp]
		public void Setup()
		{
			var container = new WindsorContainer();
			container.AddComponent("Repository", typeof(IRepository<>), typeof(Repository<>));
			container.AddComponent<IOfferRepository, OfferRepository>();
			container.AddComponent("RepositoryInterceptor", typeof(RepositoryInterceptor));
			var holder = new SessionFactoryHolder();
			holder
				.Configuration
				.Configure()
				.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Models")));
			holder.BuildSessionFactory();
			container.Kernel.AddComponentInstance<ISessionFactoryHolder>(holder);

			IoC.Initialize(container);
		}

		[Test]
		public void WindsorGenericTest()
		{
			Assert.That(IoC.Resolve<IRepository<Client>>(), Is.Not.Null);
		}

		[Test]
		public void GetTest()
		{
			var client = IoC.Resolve<IRepository<Client>>().Get(2575u);
			Assert.That(client, Is.Not.Null);
			Assert.That(client.FirmCode, Is.EqualTo(2575u));
		}

		[Test]
		public void GetClientSettings()
		{
			var clientSettings = IoC.Resolve<IRepository<ClientSettings>>().Get(2575u);
			Assert.That(clientSettings, Is.Not.Null);
			Assert.That(clientSettings.ClientCode, Is.EqualTo(2575u));
		}

		[Test]
		public void SaveOrderTest()
		{
			using (new UnitOfWork())
			{
				var client = IoC.Resolve<IRepository<Client>>().Get(2575u);
				var offers = IoC.Resolve<IOfferRepository>().FindAllForSmartOrder(client);
				Assert.That(offers.Count, Is.GreaterThan(0));
				var order = new Order(offers[0].PriceList, client, false);
				order.AddOrderItem(offers[0], 1);
				IoC.Resolve<IRepository<Order>>().Save(order);
			}
		}
	}
}
