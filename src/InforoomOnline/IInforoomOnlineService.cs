using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using InforoomOnline.Logging;
using InforoomOnline.Security;

namespace InforoomOnline
{
	[ServiceContract]
	public interface IInforoomOnlineService
	{
        [OperationContract, FaultContract(typeof(ApplicationException)), OfferRowCalculator]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract, FaultContract(typeof(ApplicationException)), RowCalculator]
		DataSet GetPriceList(string[] firmName);

		[OperationContract, FaultContract(typeof(ApplicationException)), RowCalculator]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract, FaultContract(typeof(ApplicationException))]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message);
	}
}