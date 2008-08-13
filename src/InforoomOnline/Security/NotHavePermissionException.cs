using System;
using System.Runtime.Serialization;

namespace InforoomOnline.Security
{
	public class NotHavePermissionException : ApplicationException
	{
		private const string DetailMessage = "У пользователя {0} нет права работы с сервисом";

		protected NotHavePermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}

		public NotHavePermissionException(string userName)
			: base(String.Format(DetailMessage, userName))
		{
		}
	}
}
