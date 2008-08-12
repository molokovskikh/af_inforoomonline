using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Common.MySql.Tests
{
	[TestFixture]
	public class SqlBuilderFixture
	{
		private SqlBuilder builder;

		[SetUp]
		public void SetUp()
		{
			string commandText = @"
select * 
from test";
			builder = SqlBuilder.WithCommandText(commandText);
		}

		[Test]
		public void AddCriteriaTest()
		{
			builder
				.AddCriteria("test = 1");

			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
where test = 1"));
		}

		[Test]
		public void Add2CriteriaTest()
		{
			 builder
				.AddCriteria("test = 1")
				.AddCriteria("f1 = f3");

			 Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
where test = 1 and f1 = f3"));
		}

		[Test]
		public void AddCriteriaWithAddOrNotTest()
		{
			builder
				.AddCriteria("test = 4", false)
				.AddCriteria("f5 = f4", true);

			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
where f5 = f4"));
		}

		[Test]
		public void ZeroLimitTest()
		{
			builder
				.Limit(0, 0);

			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test;"));
		}

		[Test]
		public void ZeroLimitCountTest()
		{
			builder
				.Limit(1, 0);

			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
limit 1;"));

		}

		[Test]
		public void LimitTest()
		{
			builder
				.Limit(10, 35);
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
limit 10, 35;"));
		}

		[Test]
		public void AddOrderMultiColumnTest()
		{
			builder
				.AddOrderMultiColumn(new string[] { "test", "test1" }, new string[] { "asc", "desc" });

			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
ORDER BY test asc, test1 desc"));
		}

		[Test]
		public void AddOrderMultiColumnWithoutDirection()
		{
			builder
				.AddOrderMultiColumn(new string[] { "test", "test1" }, null);
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
ORDER BY test, test1"));
		}

		[Test]
		public void AddOrderNullTest()
		{
			builder
				.AddOrder(null, null);
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test"));
		}

		[Test]
		public void AddOrderMultiColumnNullTest()
		{
			builder
				.AddOrderMultiColumn(null, null);
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test"));
		}

		[Test]
		public void AddOrderWithSingleParameterTest()
		{
			builder
				.AddOrder("t1");
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test
ORDER BY t1"));
		}

		[Test]
		public void WhereEmptyCriteriaTest()
		{
			builder
				.AddCriteria("");
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test"));
		}

		[Test]
		public void WhereNullCriteriaTest()
		{
			builder
				.AddCriteria(null);
			Assert.That(builder.GetSql(), Is.EqualTo(@"
select * 
from test"));
		}

		public delegate bool Command(string commandText);
		public delegate bool AddParameters(IEnumerable<MySqlParameter> parameters);

		[Test]
		public void AddInCriteriaTest()
		{
			MockRepository repository = new MockRepository();
			IMySqlHelper helper = repository.CreateMock<IMySqlHelper>();
			ICommandHelper commandHelper = repository.CreateMock<ICommandHelper>();

			Expect
				.Call(helper.Command(null))
				.Callback(new Command(delegate(string commandText)
				                      	{
				                      		Assert.That(commandText, Is.EqualTo(@"
select * 
from test
where  (test1 = ?p0 or test1 like ?p1)  and  (test2 = ?p2) "));
				                      		return true;
				                      	}))
				.Return(commandHelper);
			Expect
				.Call(commandHelper.AddParameters(null))
				.Callback(new AddParameters(delegate(IEnumerable<MySqlParameter> parameters)
				                            	{
				                            		List<MySqlParameter> parametersList = new List<MySqlParameter>(parameters);
				                            		Assert.That(parametersList.Count, Is.EqualTo(3));

													Assert.That(parametersList[0].ParameterName, Is.EqualTo("?p0"));
				                            		Assert.That(parametersList[0].Value, Is.EqualTo("a%"));

													Assert.That(parametersList[1].ParameterName, Is.EqualTo("?p1"));
				                            		Assert.That(parametersList[1].Value, Is.EqualTo("b%"));

													Assert.That(parametersList[2].ParameterName, Is.EqualTo("?p2"));
													Assert.That(parametersList[2].Value, Is.EqualTo(1));
				                            		return true;
				                            	}))
				.Return(null);

			repository.ReplayAll();

			builder
				.AddInCriteria("test1", new string[] {"a%", "b*"})
				.AddInCriteria("test2", new int[] { 1})
				.ToCommand(helper);

			repository.VerifyAll();
		}
	}
}