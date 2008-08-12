using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;

namespace Common.Models
{
	public interface ISessionFactoryHolder
	{
		ISessionFactory SessionFactory { get; }
	}
}
