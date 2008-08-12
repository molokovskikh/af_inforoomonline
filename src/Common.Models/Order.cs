using System;
using System.Collections.Generic;
using Common.Models.Helpers;
using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "Orders.OrdersHead", Lazy = false)]
	public class Order
	{
		public Order(PriceList priceList, Client client, bool submited) : this()
		{
			ClientCode = client.FirmCode;
			RegionCode = client.RegionCode;
			PriceList = priceList;
			WriteTime = DateTime.Now;
			PriceDate = priceList.PriceDate;

			if (submited)
			{
				Submited = true;
				SubmitDate = DateTime.Now;
			}
		}

		protected Order()
		{
			OrderItems = new List<OrderItem>();
		}

		[Id(0, Name = "RowId")]
		[Generator(1, Class = "native")]
		public uint RowId { get; set; }

		[ManyToOne(ClassType = typeof (PriceList), Column = "PriceCode")]
		public PriceList PriceList { get; set; }

		[Property]
		public DateTime PriceDate { get; set; }

		[Bag(0, Lazy = false, Inverse = true, Cascade = CascadeStyle.All)]
		[Key(1, Column = "OrderId")]
		[OneToMany(2, ClassType = typeof (OrderItem))]
		public IList<OrderItem> OrderItems { get; protected set; }

		[Property]
		public uint ClientCode { get; set; }

		[Property]
		public uint RegionCode { get; set; }

		[Property]
		public DateTime WriteTime { get; set; }

		[Property]
		public uint RowCount { get; set; }

		[Property]
		public string ClientAddition { get; set; }

		[Property]
		public bool Submited { get; protected set; }

		[Property]
		public DateTime? SubmitDate { get; set; }

		public OrderItem AddOrderItem(Offer offer, uint quantity)
		{
			RowCount++;
			OrderItems.Add(new OrderItem(this, offer, quantity));
			return OrderItems[OrderItems.Count - 1];
		}

		public float CalculateSum()
		{
			float result = 0;
			foreach (var item in OrderItems)
				result += item.Cost*item.Quantity;
			return result;
		}

		public override string ToString()
		{
			return
				String.Format(
					"RowId = {0}, PriceList = ({1}), PriceDate = {2}, ClientCode = {3}, RegionCode = {4}, WriteTime = {5}, RowCount = {6}, OrderItems = (({7}))",
					RowId,
					PriceList,
					PriceDate,
					ClientCode,
					RegionCode,
					WriteTime,
					RowCount,
					LoggingHelper.CollectionToString(OrderItems));
		}
	}
}