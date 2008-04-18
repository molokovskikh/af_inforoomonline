using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using InforoomOnline.Logging;

namespace InforoomOnline
{
	//[ServiceContract(Namespace = "http://ios.analit.net/InforoomOnLine/InforoomOnlineService.svc?wsdl")]
	[ServiceContract]
	public interface IInforoomOnlineService
	{
        [OperationContract, OfferRowCalculator]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract, RowCalculator]
		DataSet GetPriceList(string[] firmName);

        [OperationContract, RowCalculator]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message);
	}
}