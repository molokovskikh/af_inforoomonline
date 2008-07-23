using System;
using System.ServiceModel;
using Castle.Core.Interceptor;
using Common.Models;
using Common.Models.Repositories;
using InforoomOnline.Models;
using log4net;

namespace InforoomOnline.Logging
{
    public class ResultLogingInterceptor : IInterceptor
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (ResultLogingInterceptor));

        public void Intercept(IInvocation invocation)
        {
            var begin = DateTime.Now;
            invocation.Proceed();
            try
            {
                var rowCalculator =
                    (RowCalculatorAttribute)
                    Attribute.GetCustomAttribute(invocation.Method, typeof (RowCalculatorAttribute), true);

                var serviceLogEntity = new ServiceLogEntity
                                           {
                                               LogTime = DateTime.Now,
                                               MethodName = invocation.Method.Name,
                                               ProcessingTime = (DateTime.Now - begin).Milliseconds,
                                               RowCount =
                                                   rowCalculator != null
                                                       ? rowCalculator.GetRowCount(invocation.ReturnValue)
                                                       : 0,
											   UserName = ServiceSecurityContext.Current.PrimaryIdentity.Name,
                                               ServiceName = "InforoomOnline",
                                           };
                serviceLogEntity
                    .SerializeArguments(invocation.Arguments)
                    .GetHostFromOperationContext(OperationContext.Current);
                IoC.Resolve<IRepository<ServiceLogEntity>>().Save(serviceLogEntity);
            }
            catch(Exception e)
            {
                _log.Error("Ошибка логирования работы сервиса", e);
            }
        }
    }
}