using System;
using System.ServiceModel;
using Castle.Core;
using Castle.Windsor;
using Common.Models;
using InforoomOnline.Models;
using InforoomOnline.Security;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace InforoomOnline.Tests
{

	[TestFixture]
	public class PermissionCheckInterceptorFixture
	{
		[Interceptor(typeof(PermissionCheckInterceptor))]
		public class TestClass
		{
			public Action DoRun;

			public virtual void Run()
			{
				DoRun();
			}
		}

		private ISecurityRepository _securityRepository;

		[SetUp]
		public void SetUp()
		{
			ServiceContext.GetUserName = () =>"kvasov";
			_securityRepository = MockRepository.GenerateMock<ISecurityRepository>();
			var container = new WindsorContainer();
			IoC.Initialize(container);
			IoC.Container.AddComponent<PermissionCheckInterceptor>();
			IoC.Container.AddComponent<TestClass>();
			IoC.Container
				.Kernel
				.AddComponentInstance("SecurityRepository", typeof(ISecurityRepository), _securityRepository);
		}

		[TearDown]
		public void TearDown()
		{
			IoC.Container.Dispose();
			IoC.Initialize(null);
		}

		[Test]
		public void Return_null_if_do_not_have_iol_permission()
		{
			try
			{
				_securityRepository.Stub(repository => repository.HavePermission("AOL", "kvasov")).Return(false);
				var testClass = IoC.Resolve<TestClass>();
				testClass.DoRun = () => Assert.Fail("Вызвали метод хотя не должны были");
				testClass.Run();
				Assert.Fail("должны были выкинуть исключение");
			}
			catch(FaultException<NotHavePermissionException> e)
			{
				Assert.That(e.Detail.Message, Is.EqualTo("У пользователя kvasov нет права работы с сервисом"));
				Assert.That(e.Reason.GetMatchingTranslation().Text, Is.EqualTo("Нет права доступа"));
			}
		}

		[Test]
		public void Execute_method_if_user_have_permission()
		{
			_securityRepository.Stub(repository => repository.HavePermission("kvasov", "AOL")).Return(true);
			var testClass = IoC.Resolve<TestClass>();
			var isInvoked = false;
			testClass.DoRun = () => isInvoked = true;
			testClass.Run();
			Assert.That(isInvoked);
		}
	}
}
