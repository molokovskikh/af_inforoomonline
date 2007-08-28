using System.Data;
using System.ServiceModel;

namespace InforoomOnline
{
	[ServiceContract(Namespace = "InforoomOnline")]
	public interface IInforoomOnlineService
	{
		[OperationContract]
		DataSet GetOffers(string[] rangeField, string[] rangeValue, bool newEar, string[] sortField, string[] sortOrder,
		                  int limit, int selStart);

		[OperationContract]
		DataSet GetPriceList(string[] firmName);

		[OperationContract]
		DataSet GetNamesFromCatalog(string[] name, string[] form, bool offerOnly, int limit, int selStart);
	}
}