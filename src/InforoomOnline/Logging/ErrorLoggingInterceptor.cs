using System;
using Castle.Core.Interceptor;
using log4net;

namespace InforoomOnline.Logging
{
	public class ErrorLoggingInterceptor : IInterceptor
	{
		private readonly ILog _log = LogManager.GetLogger(typeof (ErrorLoggingInterceptor));

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