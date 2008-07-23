using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using InforoomOnline.Logging;
using InforoomOnline.Security;

namespace InforoomOnline
{
	//[ServiceContract(Namespace = "http://ios.analit.net/InforoomOnLine/InforoomOnlineService.svc?wsdl")]
	[ServiceContract]
//	[FaultContract(typeof(NotHavePermissionException))]
	public interface IInforoomOnlineService
	{
        [OperationContract, FaultContract(typeof(NotHavePermissionException)), OfferRowCalculator]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract, FaultContract(typeof(NotHavePermissionException)), RowCalculator]
		DataSet GetPriceList(string[] firmName);

		[OperationContract, FaultContract(typeof(NotHavePermissionException)), RowCalculator]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract, FaultContract(typeof(NotHavePermissionException))]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message);
	}
}