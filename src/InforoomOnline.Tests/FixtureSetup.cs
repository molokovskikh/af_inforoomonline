using System.IO;
using CassiniDev;
using NUnit.Framework;

namespace InforoomOnline.Tests
{
	[SetUpFixture]
	public class FixtureSetup
	{
		private Server server;

		[OneTimeSetUp]
		public void Setup()
		{
			server = new Server(54860, Path.GetFullPath(@"..\..\..\InforoomOnline"));
			server.Start();
			Test.Support.Setup.Initialize("Main");
		}

		[OneTimeTearDown]
		public void Teardown()
		{
			if (server != null)
				server.Dispose();
		}
	}
}