using System;
using System.Data;
using System.ServiceModel;
using Common.Service;

namespace InforoomOnline
{
	[ServiceContract]
	public interface IInforoomOnlineService
	{
		[OperationContract, FaultContract(typeof(DoNotHavePermissionFault)), OfferRowCalculator("FullCode")]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract, FaultContract(typeof(DoNotHavePermissionFault)), RowCalculator]
		DataSet GetPriceList(string[] firmName);

		[OperationContract, FaultContract(typeof(DoNotHavePermissionFault)), RowCalculator]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract, FaultContract(typeof(DoNotHavePermissionFault))]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message, uint addressId);
	}
}