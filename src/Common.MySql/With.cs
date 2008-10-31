using System;
using System.Configuration;
using System.Data;
using Common.Tools;
using log4net;
using MySql.Data.MySqlClient;

namespace Common.MySql
{
	public delegate void InTransactionDelegate(MySqlHelper helper);
	public delegate void Action();

	public class With
	{
		private static ILog _log = LogManager.GetLogger(typeof(With));

		public static void Transaction(InTransactionDelegate inTransaction)
		{
			DeadlockWraper(() => {
			               	using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString))
			               	{
			               		connection.Open();

			               		var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
			               		try
			               		{
			               			inTransaction(new MySqlHelper(connection, transaction));
			               			transaction.Commit();
			               		}
			               		catch (Exception)
			               		{
			               			transaction.Rollback();
			               			throw;
			               		}
			               	}});
		}

		public static void DeadlockWraper(Action action)
		{
			var done = false;
			var iteration = 0;
			var beginOn = DateTime.Now;
			while (!done && iteration < 50 && (DateTime.Now - beginOn < TimeSpan.FromSeconds(3)))
			{
				iteration++;
				try
				{
					action();
					done = true;
				}
				catch (Exception e)
				{
					if (ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(e))
					{
						_log.Warn("Deadlock, повторяем попытку {0} из 50".Format(iteration), e);
						done = false;
					}
					else
						throw;
				}
			}
		}
	}
}
