using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace Common.MySql
{
	public delegate void InTransactionDelegate(MySqlHelper helper);

	public class With
	{
		public static void Transaction(InTransactionDelegate inTransaction)
		{
			using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString))
			{
				connection.Open();

				MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
				try
				{
					inTransaction(new MySqlHelper(connection, transaction));
					transaction.Commit();
				}
				catch
				{
					transaction.Rollback();
					throw;
				}
			}
		}
	}
}
