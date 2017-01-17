using System.IO;
using CassiniDev;
using Common.MySql;
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
			ConnectionHelper.DefaultConnectionStringName = "Main";
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