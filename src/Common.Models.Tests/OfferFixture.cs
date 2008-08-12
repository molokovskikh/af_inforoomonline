using Common.Models;
using NUnit.Framework;

namespace Common.Models.Tests
{
	[TestFixture]
	public class OfferFixture
	{
		[Test]
		public void OrderCostTest()
		{
			Offer offer = new Offer();
			offer.Cost = 10;
			offer.OrderCost = null;
			Assert.IsTrue(offer.IsOrderCostValide(1), "null");
			offer.OrderCost = 0;
			Assert.IsTrue(offer.IsOrderCostValide(1), "0");
			offer.OrderCost = 12;
			Assert.IsFalse(offer.IsOrderCostValide(1), "ordercost = 12, quantity = 1");
			Assert.IsTrue(offer.IsOrderCostValide(2), "ordercost = 12, quantity = 2");
		}

		[Test]
		public void GetRequestRationRemainderTest()
		{
			Offer offer = new Offer();
			offer.RequestRatio = null;

			uint remainder;
			Assert.AreEqual(offer.GetRequestRationRemainder(11, out remainder), true);
			Assert.AreEqual(remainder, 0);

			offer.RequestRatio = 0;
			Assert.AreEqual(offer.GetRequestRationRemainder(11, out remainder), true);
			Assert.AreEqual(remainder, 0);

			offer.RequestRatio = 3;
			Assert.AreEqual(offer.GetRequestRationRemainder(9, out remainder), true);
			Assert.AreEqual(remainder, 0);

			offer.RequestRatio = 5;
			Assert.AreEqual(offer.GetRequestRationRemainder(11, out remainder), true);
			Assert.AreEqual(remainder, 1);

			Assert.AreEqual(offer.GetRequestRationRemainder(3, out remainder), false);
			Assert.AreEqual(remainder, 0);
		}

		[Test]
		public void IsMinOrderCountValideTest()
		{
			Offer offer = new Offer();
			offer.MinOrderCount = null;
			Assert.IsTrue(offer.IsMinOrderCountValide(10));
			offer.MinOrderCount = 10;
			Assert.IsFalse(offer.IsMinOrderCountValide(5));
			Assert.IsTrue(offer.IsMinOrderCountValide(10));
			Assert.IsTrue(offer.IsMinOrderCountValide(15));
		}
	}
}