using System;
using System.Reflection;
using System.Threading;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.MySql.Tests
{
	[TestFixture]
	public class WithTransactionFixture
	{
		[Test]
		public void If_deadlock_occur_try_repeat_50_times_but_not_longer_than_5_seconds()
		{
			var i = 0;

			try
			{
				With.Transaction(helper => {
				                 	i++;
				                 	throw GetMySqlException(1205, "");
				                 });
			}
			catch (MySqlException e)
			{
				Assert.That(e.Number, Is.EqualTo(1205), e.ToString());
			}
			Assert.That(i, Is.EqualTo(50));

			i = 0;
			try
			{
				With.Transaction(helper => {
				                 	i++;
				                 	Thread.Sleep(100);
				                 	throw GetMySqlException(1205, "");
				                 });
			}
			catch (MySqlException e)
			{
				Assert.That(e.Number, Is.EqualTo(1205), e.ToString());
			}
			Assert.That(i, Is.EqualTo(30));
		}

		public static MySqlException GetMySqlException(int errorCode, string message)
		{
			return (MySqlException)typeof(MySqlException)
									.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
													null,
													new Type[] { typeof(string), typeof(int) },
													null)
									.Invoke(new object[] { message, errorCode });
		}

		public static Exception PutExceptionInStack(Exception exception, int depth)
		{
			if (depth >= 1)
			{
				Exception currentLevelException = new Exception("Exception level " + depth, exception);
				return PutExceptionInStack(currentLevelException, depth - 1);
			}
			return exception;
		}
	}
}
