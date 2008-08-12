using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core;
using NHibernate;

namespace Common.Models.Repositories
{
	[Interceptor(typeof(RepositoryInterceptor))]
	public class BaseRepository
	{
		protected static ISession CurrentSession
		{
			get
			{
				if (UnitOfWork.Current == null)
					throw new Exception("UnitOfWork �� ����������������");
				return UnitOfWork.Current.CurrentSession;
			}
		}
	}
}
