using System;
using System.ServiceModel;

namespace InforoomOnline.Security
{
	public class NotHavePermissionException : FaultException<string>
	{
		private const string DetailMessage = "У пользователя {0} нет права работы с сервисом";
		private const string ReasonMessage = "У пользователя {0} нет права работы с сервисом";

		public NotHavePermissionException(string userName)
			: base(String.Format(DetailMessage, userName),
			       String.Format(ReasonMessage, userName))
		{
		}
	}
}
