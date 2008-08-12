using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
	//заготовка на будущее что бы унифицировать процесс заказа
/*	public class OrderBuilder
	{
		public static IList<Order> BuildOrders(IList<ItemToOrder> itemToOrders, Client client)
		{
			List<Order> orders = new List<Order>();
			foreach (ItemToOrder itemToOrder in itemToOrders)
			{				
				uint quantityCorrection;
				if (itemToOrder.Offer != null 
					&& itemToOrder.Offer.IsOrderCostValide(itemToOrder.Quantity)
					&& itemToOrder.Offer.IsMinOrderCountValide(itemToOrder.Quantity)
					&& itemToOrder.Offer.GetRequestRationRemainder(itemToOrder.Quantity, out quantityCorrection))
				{
					Order order = orders.Find(delegate(Order obj)
					                          	{
					                          		return obj.PriceList == itemToOrder.Offer.PriceList;
					                          	});
					if (order == null)
						order = new Order(itemToOrder.Offer.PriceList, client); 

					itemToOrder.OrderItem = new OrderItem(order, itemToOrder.Offer, itemToOrder.Quantity - quantityCorrection);
				}
			}
			return orders;
		}
	}*/
}
