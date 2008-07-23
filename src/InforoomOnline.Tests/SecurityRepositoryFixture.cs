using System.Configuration;
using Common.Models;
using Common.Models.Repositories;
using InforoomOnline.Models;
using InforoomOnline.Tests.ForTesting;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class SecurityRepositoryFixture : FixtureWithIoC
	{
		private ISecurityRepository repository;

		public override void SetUp()
		{
			base.SetUp();

			IoC.Container.AddComponent<ISecurityRepository, SecurityRepository>();
			IoC.Container.AddComponent<RepositoryInterceptor>();
			IoC.Container.AddComponent<ISessionFactoryHolder, SessionFactoryHolder>();

			var sessionFactory = (SessionFactoryHolder) IoC.Resolve<ISessionFactoryHolder>();
			sessionFactory
				.Configuration
				.Configure();
			sessionFactory.BuildSessionFactory();

			repository = IoC.Resolve<ISecurityRepository>();
		}

		public override void TearDown()
		{
			base.TearDown();

			if (UnitOfWork.Current != null)
				UnitOfWork.Current.Dispose();
		}


		[Test]
		public void HavePermissionTest()
		{
			Execute(@"
delete ap from AssignedPermissions ap, osuseraccessright oar, userpermissions up
where ap.permissionid = up.id and oar.rowid = ap.userid and oar.osusername = 'kvasov' and up.shortcut = 'AOL';");
			Assert.That(repository.HavePermission("kvasov", "AOL"), Is.False);

			Execute(
				@"
insert into AssignedPermissions(userid, permissionid)
select oar.rowid, up.id
from osuseraccessright oar, userpermissions up
where oar.osusername = 'kvasov' and up.shortcut = 'AOL';");
			Assert.That(repository.HavePermission("kvasov", "AOL"), Is.True);
		}

		public void Execute(string commandText)
		{
			using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = commandText;
				command.ExecuteNonQuery();
			}
		}
	}
}
