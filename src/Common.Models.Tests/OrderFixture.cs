using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Models.Tests
{
	[TestFixture]
	public class OrderFixture
	{
		[Test]
		public void CalculateSumTest()
		{
			var priceList = new PriceList();
			var client = new Client();
			var offer1 = new Offer {Cost = 3.2f};
			var offer2 = new Offer {Cost = 50.0f};
			var order = new Order(priceList, client, false);
			order.AddOrderItem(offer1, 1);
			order.AddOrderItem(offer2, 15);
			Assert.AreEqual(753.2f, order.CalculateSum());
		}

		[Test]
		public void Set_submited_and_submit_date_if_submited()
		{
			var begin = DateTime.Now;
			var price = new PriceList();
			var client = new Client();
			var order = new Order(price, client, true);
			Assert.That(order.Submited);
			Assert.That(order.SubmitDate, Is.GreaterThanOrEqualTo(begin));
		}
	}
}