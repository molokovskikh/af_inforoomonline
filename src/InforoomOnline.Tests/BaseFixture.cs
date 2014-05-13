using System.Linq;
using System.Reflection;
using Castle.ActiveRecord;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common.Models;
using Common.Models.Repositories;
using Common.Service;
using Common.Service.Interceptors;
using Common.Tools;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using Test.Support;
using Test.Support.Suppliers;

namespace InforoomOnline.Tests
{
	public class BaseFixture : Test.Support.IntegrationFixture
	{
		protected TestClient client;
		protected TestUser user;
		protected TestAddress address;
		protected SessionScope scope;
		protected IInforoomOnlineService service;

		[SetUp]
		public void Setup()
		{
			var supplier = TestSupplier.CreateNaked(session);
			supplier.CreateSampleCore(session);
			client = TestClient.CreateNaked(session);
			address = client.CreateAddress();
			user = client.Users.First();
			session.Save(address);
			ServiceContext.GetUserName = () => user.Login;

			var container = new WindsorContainer();
			container.AddComponent("RepositoryInterceptor", typeof(RepositoryInterceptor));
			container.AddComponent("OfferRepository", typeof(IOfferRepository), typeof(OfferRepository));
			container.AddComponent("Repository", typeof(IRepository<>), typeof(Repository<>));
			var holder = new SessionFactoryHolder();
			holder
				.Configuration
				.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Models")));
			container.Kernel.AddComponentInstance<ISessionFactoryHolder>(holder);
			IoC.Initialize(container);
			IoC.Container.Register(
				Component.For<IInforoomOnlineService>()
					.ImplementedBy<InforoomOnlineService>()
					.Interceptors(InterceptorReference.ForType<ContextLoaderInterceptor>())
					.Anywhere,
				Component.For<ContextLoaderInterceptor>(),
				Component.For<IClientLoader>().ImplementedBy<ClientLoader>());

			service = IoC.Resolve<IInforoomOnlineService>();
		}

		[TearDown]
		public void TearDown()
		{
			scope.SafeDispose();
		}
	}
}