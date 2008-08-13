using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Models;
using Common.Models.Repositories;
using NHibernate.Expression;

namespace InforoomOnline.Tests.ForTesting
{
	public static class RepositoryExtensions
	{
		public static T FindOne<T>(this IRepository<T> repository, params ICriterion[] criterions)
		{
			if (UnitOfWork.Current == null)
			{
				using (var unitOfWork = new UnitOfWork())
				{
					var criteria = unitOfWork.CurrentSession.CreateCriteria(typeof (T));
					foreach (var criterion in criterions)
						criteria.Add(criterion);
					return criteria.UniqueResult<T>();
				}
			}
			else
			{
				var criteria = UnitOfWork.Current.CurrentSession.CreateCriteria(typeof(T));
				foreach (var criterion in criterions)
					criteria.Add(criterion);
				return criteria.UniqueResult<T>();				
			}
		}
	}
}
