using System;
using System.Collections.Generic;
using System.Text;
using Castle.Windsor;

namespace Common.Models
{
	public class IoC
	{
		private static IWindsorContainer _container;

		public static void Initialize(IWindsorContainer container)
		{
			_container = container;
		}

		public static IWindsorContainer Container
		{
			get { return _container; }
		}

		public static T Resolve<T>()
		{
			return _container.Resolve<T>();
		}
	}
}
