using System;
using MySql.Data.MySqlClient;

namespace Common.Tools
{
	public class ExceptionHelper
	{
		public static bool IsDeadLockOrSimilarExceptionInChain(Exception ex)
		{
			if (ex == null)
				return false;
			if (ex is MySqlException 
				&& (((MySqlException)ex).Number == 1205 
					|| ((MySqlException)ex).Number == 1213 
					|| ((MySqlException)ex).Number == 1422))
				return true;
			else
				return IsDeadLockOrSimilarExceptionInChain(ex.InnerException);
		}
	}
}
