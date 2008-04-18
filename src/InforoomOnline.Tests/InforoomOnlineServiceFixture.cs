using System;
using System.Data;
using NUnit.Framework;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class InforoomOnlineServiceFixture
    {
        [Test]
        public void GetNameFromCatalog()
        {
            var service = new InforoomOnlineService();
            LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0));
            LogDataSet(service.GetNamesFromCatalog(new[] {"*����*"}, new string[0], false, 100, 0));
            LogDataSet(service.GetNamesFromCatalog(new string[0], new[] {"*����*"}, false, 100, 0));
            LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0));
            LogDataSet(service.GetNamesFromCatalog(new[] {"*����*"}, new string[0], true, 100, 0));
            LogDataSet(service.GetNamesFromCatalog(new string[0], new[] {"*����*"}, true, 100, 0));
        }

        [Test]
        public void GetOffers()
        {
            var service = new InforoomOnlineService();
            var data = service.GetOffers(null, null, false,
                                             new string[0], new string[0], 100, 0);
            LogDataSet(data);
        }

        [Test]
        public void GetPriceList()
        {
            var service = new InforoomOnlineService();
            LogDataSet(service.GetPriceList(new string[0]));
            LogDataSet(service.GetPriceList(new[] {"%�%"}));
        }

        [Test]
        public void PostOrder()
        {
            var service = new InforoomOnlineService();
            service.PostOrder(new long[] {687471520}, new[] {1}, new[] {"��� �������� �����"});
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
