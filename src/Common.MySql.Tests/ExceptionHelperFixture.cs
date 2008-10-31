using System;
using System.Reflection;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Common.MySql.Tests
{
	[TestFixture]
	public class ExceptionHelperFixture
	{
		[Test]
		public void IsDeadLockOrSimilarExceptionInChainTest()
		{
			var exception1 = PutExceptionInStack(GetMySqlException(1213, "Deadlock 1"), 3);
			var exception2 = PutExceptionInStack(GetMySqlException(1205, "Deadlock 2"), 5);
			var exception3 = PutExceptionInStack(GetMySqlException(1422, "Deadlock 3"), 4);

			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception1));
			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception2));
			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception3));
		}

		public static MySqlException GetMySqlException(int errorCode, string message)
		{
			return (MySqlException)typeof(MySqlException)
			                       	.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
			                       	                null,
			                       	                new[] { typeof(string), typeof(int) },
			                       	                null)
			                       	.Invoke(new object[] { message, errorCode });
		}

		public static Exception PutExceptionInStack(Exception exception, int depth)
		{
			if (depth >= 1)
			{
				var currentLevelException = new Exception("Exception level " + depth, exception);
				return PutExceptionInStack(currentLevelException, depth - 1);
			}
			return exception;
		}
	}
}