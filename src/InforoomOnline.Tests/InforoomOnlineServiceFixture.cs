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
		}

		[Test]
		public void GetOffers()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			DataSet data = service.GetOffers(new string[] { "PriceCode", "Code" }, new string[] { "32", "нт-659" }, false, new string[0], new string[0], 100, 0);
		}

		[Test]
		public void GetPriceList()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			service.GetPriceList(new string[0]);
		}
	}
}
