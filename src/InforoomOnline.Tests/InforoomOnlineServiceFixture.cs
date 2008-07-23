using System;
using System.Data;
using System.Linq;
using System.ServiceModel;
using InforoomOnline.Security;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class InforoomOnlineServiceFixture
    {
    	private InforoomOnlineService _service;
    	private string[] Empty;

		public T[] Array<T>(params T[] items)
		{
			return items;
		}

    	[SetUp]
		public void Setup()
		{
			_service = new InforoomOnlineService();
    		ServiceContext.GetUserName = () => "kvasov";
		}

    	[Test]
        public void GetNameFromCatalog()
        {
    		LogDataSet(_service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, false, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], true, 100, 0));
            LogDataSet(_service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, true, 100, 0));
        }

        [Test]
        public void GetOffers()
        {
            var data = _service.GetOffers(null, null, false,
                                             new string[0], new string[0], 100, 0);
            LogDataSet(data);
        }

        [Test]
        public void GetPriceList()
        {
            LogDataSet(_service.GetPriceList(new string[0]));
            LogDataSet(_service.GetPriceList(new[] {"%а%"}));
        }

        [Test]
        public void PostOrder()
        {
			var data = _service.GetOffers(Array("name"), Array("*папа*"), false, Empty, Empty, 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0), "не нашли предложений");
        	var row = data.Tables[0].Rows[0];
        	LogDataSet(_service.PostOrder(Array(Convert.ToInt64(row["OfferId"])),
        	                              Array(1),
        	                              Array("это тестовый заказ")));
        }

		[Test]
		public void All_methods_must_be_marked_with_fault_contract_attribute()
		{
			foreach(var method in typeof(IInforoomOnlineService).GetMethods())
			{
				var finded = false;
				foreach (var attribute in method.GetCustomAttributes(typeof(FaultContractAttribute), true))
				{
					finded = ((FaultContractAttribute) attribute).DetailType == typeof (NotHavePermissionException);
					if (finded)
						break;
				}

				Assert.That(finded, String.Format("Метод {0} не содержит атрибута FaultContract с типом NotHavePermissionException", method.Name));
			}
		}

    	private static void LogDataSet(DataSet dataSet)
        {
            foreach (DataTable dataTable in dataSet.Tables)
            {
                Console.WriteLine("<table>");
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    Console.WriteLine("\t<row>");
                    Console.Write("\t");
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        Console.Write(dataRow[column] + " ");
                    }
                    Console.WriteLine();
                    Console.WriteLine("\t</row>");
                }
                Console.WriteLine("</table>");
            }
        }
    }
}
