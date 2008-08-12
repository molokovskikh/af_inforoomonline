using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Interceptor;

namespace Common.Models.Repositories
{
	public class RepositoryInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			if (UnitOfWork.Current == null)
				using(new UnitOfWork())
					invocation.Proceed();
			else 
				invocation.Proceed();
		}
	}
}
