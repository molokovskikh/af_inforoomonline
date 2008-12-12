using NHibernate;
using NHibernate.Cfg;

namespace Common.Models
{
    public class SessionFactoryHolder : ISessionFactoryHolder
    {
	    private readonly Configuration _configuration;
    	private ISessionFactory _sessionFactory;
		private readonly object _sessionFactoryLock = new object();

	    public SessionFactoryHolder()
		{
			_configuration = new Configuration();
		}

	    public Configuration Configuration
	    {
            get { return _configuration; }
	    }

		public ISessionFactory SessionFactory
		{
			get
			{
				if (_sessionFactory == null)
					lock (_sessionFactoryLock)
						if (_sessionFactory == null)
							_sessionFactory = _configuration.BuildSessionFactory();
				return _sessionFactory;
			}
		}
	}
}
