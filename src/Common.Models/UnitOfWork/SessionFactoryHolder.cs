using NHibernate;
using NHibernate.Cfg;

namespace Common.Models
{
    public class SessionFactoryHolder : ISessionFactoryHolder
    {
	    private readonly Configuration _configuration;

	    public SessionFactoryHolder()
		{
			_configuration = new Configuration();
		}

        public void BuildSessionFactory()
        {
            SessionFactory = _configuration.BuildSessionFactory();
        }

	    public Configuration Configuration
	    {
            get { return _configuration; }
	    }

        public ISessionFactory SessionFactory{ get; private set; }
	}
}
