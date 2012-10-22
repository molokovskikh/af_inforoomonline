using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Interceptor;
using log4net;

namespace InforoomOnline
{
	public class LoggingInterceptor : IInterceptor
	{
		private readonly ILog _log = LogManager.GetLogger(typeof (LoggingInterceptor));

		public void Intercept(IInvocation invocation)
		{
			try
			{
				invocation.Proceed();
			}
			catch(Exception ex)
			{
				_log.Error("Ошибка в сервисе InforoomOnline", ex);
				throw;
			}
		}
	}
}
