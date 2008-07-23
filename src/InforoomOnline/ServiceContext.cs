using System;
using System.ServiceModel;

namespace InforoomOnline
{
	public static class ServiceContext
	{
		public static Func<string> GetUserName = () => ServiceSecurityContext.Current.PrimaryIdentity.Name;
	}
}
