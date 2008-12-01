using System;
using System.Data;
using System.ServiceModel;
using Common.Service;

namespace InforoomOnline
{
	[ServiceContract]
	public interface IInforoomOnlineService
	{
		[OperationContract, FaultContract(typeof(FaultMessage)), OfferRowCalculator]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract, FaultContract(typeof(FaultMessage)), RowCalculator]
		DataSet GetPriceList(string[] firmName);

		[OperationContract, FaultContract(typeof(FaultMessage)), RowCalculator]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract, FaultContract(typeof(FaultMessage))]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message);
	}
}