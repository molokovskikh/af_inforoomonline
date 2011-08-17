using NUnit.Framework;

namespace InforoomOnline.Tests
{
	[SetUpFixture]
	public class FixtureSetup
	{
		[SetUp]
		public void Setup()
		{
			Test.Support.Setup.Initialize("Main");
		}
	}
}