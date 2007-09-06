using System;
using System.Collections.Generic;
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
			service.GetOffers(new string[0], new string[0], false, new string[0], new string[0], 100, 0);
		}

		[Test]
		public void GetPriceList()
		{
			InforoomOnlineService service = new InforoomOnlineService();
			service.GetPriceList(new string[0]);
		}
	}
}
