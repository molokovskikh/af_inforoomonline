using System;
using System.ServiceModel;
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
				if (!(ex is FaultException))
					_log.Error("������ � ������� InforoomOnline", ex);
				throw;
			}
		}
	}
}