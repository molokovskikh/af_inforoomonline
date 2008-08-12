using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Models.Tests
{
	[TestFixture]
	public class OrderItemFixture
	{
		[Test]
		public void Create_order_item_from_offer()
		{
			var order = new Order(new PriceList(), new Client(), false);
			var offer = new Offer
			            	{
								Id = 654654879879,
			            		RequestRatio = 10,
								MinOrderCount = 100,
								OrderCost = 1.5f,
			            	};
			var orderItem = new OrderItem(order, offer, 10);
			Assert.That(orderItem.CoreId, Is.EqualTo(offer.Id));
			Assert.That(orderItem.OrderCost, Is.EqualTo(offer.OrderCost));
			Assert.That(orderItem.MinOrderCount, Is.EqualTo(offer.MinOrderCount));
			Assert.That(orderItem.RequestRatio, Is.EqualTo(offer.RequestRatio));
		}
	}
}
