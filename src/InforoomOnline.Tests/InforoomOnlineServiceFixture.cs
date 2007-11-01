using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NUnit.Framework;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class InforoomOnlineServiceFixture
	{
		[Test]
		public void GetNameFromCatalog()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[] { "*����*" }, new string[0], false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[] { "*����*" }, false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[] { "*����*" }, new string[0], true, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[] { "*����*" }, true, 100, 0));
		}

		[Test]
		public void GetOffers()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			DataSet data = service.GetOffers(new string[] { "PriceCode", "Code", "FullCode" }, new string[] { "32", "��-659", "1" }, false, new string[0], new string[0], 100, 0);
		}

		[Test]
		public void GetPriceList()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			LogDataSet(service.GetPriceList(new string[0]));
			LogDataSet(service.GetPriceList(new string[] {"%�%"}));
		}

		[Test]
		public void PostOrder()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			service.PostOrder(new long[] { 687471520 }, new int[] { 1 }, new string[] { "��� �������� �����" });
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
