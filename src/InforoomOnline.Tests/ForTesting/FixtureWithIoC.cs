using Castle.Windsor;
using Common.Models;
using NUnit.Framework;

namespace InforoomOnline.Tests.ForTesting
{
	[TestFixture]
	public class FixtureWithIoC
	{
		[SetUp]
		public virtual void SetUp()
		{
			var container = new WindsorContainer();
			IoC.Initialize(container);
		}

		[TearDown]
		public virtual void TearDown()
		{
			IoC.Container.Dispose();
			IoC.Initialize(null);
		}
	}
}
