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
			service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0);
			service.GetNamesFromCatalog(new string[] { "*Тест*" }, new string[0], false, 100, 0);
			service.GetNamesFromCatalog(new string[0], new string[] { "*Тест*" }, false, 100, 0);
			service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0);
			service.GetNamesFromCatalog(new string[] { "*Тест*" }, new string[0], true, 100, 0);
			service.GetNamesFromCatalog(new string[0], new string[] { "*Тест*" }, true, 100, 0);
		}

		[Test]
		public void GetOffers()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			DataSet data = service.GetOffers(new string[] { "PriceCode", "Code", "FullCode" }, new string[] { "32", "ОФ-659", "1" }, false, new string[0], new string[0], 100, 0);
		}

		[Test]
		public void GetPriceList()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			service.GetPriceList(new string[0]);
			service.GetPriceList(new string[] {"%а%"});
		}

		[Test]
		public void PostOrder()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			service.PostOrder(new long[] { 687471520 }, new int[] { 1 }, new string[] { "это тестовый заказ" });
		}
	}
}
