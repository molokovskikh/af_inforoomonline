using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Common.Tools.Test
{
	[TestFixture]
	public class ExceptionHelperFixture
	{
		[Test]
		public void IsDeadLockOrSimilarExceptionInChainTest()
		{
			Exception exception1 = PutExceptionInStack(GetMySqlException(1213, "Deadlock 1"), 3);
			Exception exception2 = PutExceptionInStack(GetMySqlException(1205, "Deadlock 2"), 5);
			Exception exception3 = PutExceptionInStack(GetMySqlException(1422, "Deadlock 3"), 4);

			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception1));
			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception2));
			Assert.That(ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(exception3));
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