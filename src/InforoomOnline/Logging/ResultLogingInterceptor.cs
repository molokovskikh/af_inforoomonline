using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Text;
using Castle.Core.Interceptor;
using InforoomOnline.Models;

namespace InforoomOnline.Logging
{
	public class ResultLogingInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			DateTime begin = DateTime.Now;
			invocation.Proceed();
			RowCalculatorAttribute rowCalculator =
				(RowCalculatorAttribute) Attribute.GetCustomAttribute(invocation.Method, typeof (RowCalculatorAttribute), true);
			if (rowCalculator != null)
			{
				ServiceLogEntity serviceLogEntity = new ServiceLogEntity();
				serviceLogEntity.LogTime = DateTime.Now;
				serviceLogEntity.MethodName = invocation.Method.Name;
				serviceLogEntity.ProcessingTime = (begin - DateTime.Now).Milliseconds;
				serviceLogEntity.RowCount = rowCalculator.GetRowCount(invocation.ReturnValue);
				serviceLogEntity.UserName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
				serviceLogEntity.ServiceName = "";
				serviceLogEntity.Host = "";
			}
		}
	}
}