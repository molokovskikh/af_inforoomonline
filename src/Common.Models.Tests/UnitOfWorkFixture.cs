using System.Threading;
using Castle.Windsor;
using NHibernate;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Common.Models.Tests
{
	[TestFixture]
	public class UnitOfWorkFixture
	{
		private MockRepository repository;
		private IWindsorContainer container;
		private ISessionFactoryHolder factoryHolder;
		private ISessionFactory sessionFactory;
		private ISession session;

		[SetUp]
		public void SetUp()
		{
			repository = new MockRepository();
			container = repository.CreateMockWithRemoting<IWindsorContainer>();
			factoryHolder = repository.CreateMock<ISessionFactoryHolder>();
			sessionFactory = repository.CreateMock<ISessionFactory>();
			session = repository.CreateMock<ISession>();
			IoC.Initialize(container);
		}

		[Test]
		public void CreateNewUnitOfWok()
		{
			using (repository.Ordered())
			{
				Expect.Call(container.Resolve<ISessionFactoryHolder>()).Return(factoryHolder);
				Expect.Call(factoryHolder.SessionFactory).Return(sessionFactory);
				Expect.Call(sessionFactory.OpenSession()).Return(session);
				session.Dispose();
			}

			repository.ReplayAll();
			var unitOfWork = new UnitOfWork();

			Assert.That(UnitOfWork.Current, Is.EqualTo(unitOfWork));
			Assert.That(unitOfWork.CurrentSession, Is.EqualTo(session));

			unitOfWork.Dispose();
			Assert.That(UnitOfWork.Current, Is.Null);

			repository.VerifyAll();
		}

		[Test]
		public void MultiThreadTest()
		{
			using (repository.Record())
			{
				SetupResult.On(container).Call(container.Resolve<ISessionFactoryHolder>()).Return(factoryHolder);
				SetupResult.On(factoryHolder).Call(factoryHolder.SessionFactory).Return(sessionFactory);
				SetupResult.On(sessionFactory).Call(sessionFactory.OpenSession()).Return(session);
				session.Dispose();
			}

			repository.ReplayAll();
			var unitOfWork = new UnitOfWork();
			var thread = new Thread(new ThreadStart(delegate
			                                        	{
			                                        		var unitOfWork1 = new UnitOfWork();
			                                        		Assert.That(UnitOfWork.Current, Is.Not.EqualTo(unitOfWork));
			                                        		Assert.That(UnitOfWork.Current, Is.EqualTo(unitOfWork1));
			                                        	}));
			thread.Start();
			thread.Join();
			Assert.That(UnitOfWork.Current, Is.EqualTo(unitOfWork));

			UnitOfWork.Current.Dispose();
			repository.VerifyAll();
		}
	}
}
