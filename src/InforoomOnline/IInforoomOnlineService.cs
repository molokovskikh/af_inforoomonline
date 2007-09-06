using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace InforoomOnline
{
	//[ServiceContract(Namespace = "http://ios.analit.net/InforoomOnLine/")]
	[ServiceContract]
	public interface IInforoomOnlineService
	{
		[OperationContract]
		DataSet GetOffers(string[] rangeField, 
						  string[] rangeValue, 
						  bool newEar, 
						  string[] sortField, 
						  string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract]
		DataSet GetPriceList(string[] firmName);

		[OperationContract]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);

		[OperationContract]
		DataSet PostOrder(long[] offerId, Int32[] quantity, string[] message);
	}
}