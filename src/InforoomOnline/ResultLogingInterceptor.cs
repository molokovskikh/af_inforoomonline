using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Castle.Core.Interceptor;

namespace InforoomOnline
{
	public class ResultLogingInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			invocation.Proceed();
			if (invocation.ReturnValue != null && invocation.ReturnValue is DataSet)
			{
				DataSet data = (DataSet) invocation.ReturnValue;
				
			}
		}
	}
}
