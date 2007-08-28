using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace InforoomOnline
{
	public class InforoomOnlineService : IInforoomOnlineService
	{
		public DataSet GetOffers(string[] rangeField, string[] rangeValue, bool newEar, string[] sortField, string[] sortOrder, int limit, int selStart)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public DataSet GetPriceList(string[] firmName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
