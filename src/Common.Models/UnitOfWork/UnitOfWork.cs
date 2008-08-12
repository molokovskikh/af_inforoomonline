using System;
using NHibernate;

namespace Common.Models
{
	public class UnitOfWork : IDisposable
	{
		[ThreadStatic]
		private static UnitOfWork _current;
		private readonly ISession _session;
		private readonly ISessionFactory _sessionFactory;

		public UnitOfWork()
		{
			_sessionFactory = IoC.Resolve<ISessionFactoryHolder>().SessionFactory;
			if (_sessionFactory == null)
				throw new Exception("SessionFactoryHolder не инициализировал SessionFactory, забыл вызвать BuildSessionFactory?");
			_current = this;
			_session = _sessionFactory.OpenSession();
		}

		public static UnitOfWork Current
		{
			get { return _current; }
		}

		public ISession CurrentSession
		{
			get { return _session; }
		}

		public void Dispose()
		{
			_current = null;			
			_session.Dispose();
		}
	}
}
