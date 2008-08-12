using System;
using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "Orders.OrdersList", Lazy = false)]
	public class OrderItem
	{
		protected OrderItem()
		{}

		public OrderItem(Order order, Offer offer, uint quantity)
		{
			Order = order;
			Cost = offer.Cost;
			Quantity = quantity;
			CoreId = offer.Id;
			ProductId = offer.ProductId;
			CodeFirmCr = offer.CodeFirmCr;
			SynonymCode = offer.SynonymCode;
			SynonymFirmCrCode = offer.SynonymFirmCrCode;
			Code = offer.Code;
			CodeCr = offer.CodeCr;
			Junk = offer.Junk;
			Await = offer.Await;
			RequestRatio = offer.RequestRatio;
			MinOrderCount = offer.MinOrderCount;
			OrderCost = offer.OrderCost;
		}

		[Id(0, Name = "RowId")]
		[Generator(1, Class = "native")]
		public uint RowId { get; set; }

		[Property]
		public uint ProductId { get; set; }

		[Property]
		public uint? CodeFirmCr { get; set; }

		[Property]
		public uint SynonymCode { get; set; }

		[Property]
		public uint? SynonymFirmCrCode { get; set; }

		[Property]
		public string Code { get; set; }

		[Property]
		public string CodeCr { get; set; }

		[Property]
		public float Cost { get; set; }

		[Property]
		public uint Quantity { get; set; }

		[Property]
		public bool Junk { get; set; }

		[Property]
		public bool Await { get; set; }

		[Property]
		public uint? MinOrderCount { get; set; }

		[Property]
		public float? OrderCost { get; set; }

		[Property]
		public uint? RequestRatio { get; set; }

		[Property]
		public ulong? CoreId { get; set; }

		[ManyToOne(ClassType = typeof (Order), Column = "OrderId")]
		public Order Order { get; set; }

		public override string ToString()
		{
			return
				String.Format(
					"ProductId = {0}, CodeFirmCr = {1}, SynonymCode = {2}, SynonymFirmCrCode = {3}, Code = {4}, CodeCr = {5}, Cost = {6}, Quantity = {7}, Junk = {8}, Await = {9}, RowId = {10}",
					ProductId,
					CodeFirmCr,
					SynonymCode,
					SynonymFirmCrCode,
					Code,
					CodeCr,
					Cost,
					Quantity,
					Junk,
					Await,
					RowId);
		}
	}
}