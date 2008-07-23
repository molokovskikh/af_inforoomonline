using Castle.Core.Interceptor;
using InforoomOnline.Models;

namespace InforoomOnline.Security
{
	public class PermissionCheckInterceptor : IInterceptor
	{
		private readonly ISecurityRepository _repository;

		public PermissionCheckInterceptor(ISecurityRepository repository)
		{
			_repository = repository;
		}

		public void Intercept(IInvocation invocation)
		{
			if (!_repository.HavePermission(ServiceContext.GetUserName(), "AOL"))
				throw new NotHavePermissionException(ServiceContext.GetUserName());

			invocation.Proceed();
		}
	}
}
