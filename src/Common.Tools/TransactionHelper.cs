using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using NHibernate;

namespace Common.Tools
{
	public delegate T Action<T>();

	public class TransactionHelper
	{
		public static void InTransaction(ITransaction transaction, Action action)
		{
				InTransaction<object>(transaction, 
					delegate
						{
							action();
				            return new object();
						});
		}

		public static T InTransaction<T>(ITransaction transaction, Action<T> action)
		{
			try
			{
				T result = action();
				transaction.Commit();
				return result;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}
	}
}