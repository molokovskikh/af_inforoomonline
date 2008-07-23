using System;
using log4net.Core;
using log4net.Filter;
using MySql.Data.MySqlClient;

namespace InforoomOnline.Logging
{
	public class DeadLockExceptionFilter : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Error)
			{
				if (Check(loggingEvent.ExceptionObject, ex => ex is MySqlException
				                                              && (((MySqlException) ex).Number == 1205
				                                                  || ((MySqlException) ex).Number == 1213
				                                                  || ((MySqlException) ex).Number == 1422)))
					return FilterDecision.Deny;
			}

			return FilterDecision.Neutral;
		}

		public bool Check(Exception exception, Func<Exception, bool> func)
		{
			if (exception == null)
				return false;

			if (func(exception))
				return true;

			return Check(exception.InnerException, func);
		}
	}
}
