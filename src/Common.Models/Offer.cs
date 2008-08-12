using System;
using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Lazy = false)]
	public class Offer
	{
		[Id(0, Name = "Id")]
		[Generator(1, Class = "native")]
		public ulong Id { get; set; }

		[Property]
		public uint ProductId { get; set; }

		[Property]
		public string Code { get; set; }

		[Property]
		public string CodeCr { get; set; }

		[Property]
		public bool Junk { get; set; }

		[Property]
		public bool Await { get; set; }

		[Property]
		public uint? CodeFirmCr { get; set; }

		[Property]
		public float Cost { get; set; }

		[Property]
		public uint SynonymCode { get; set; }

		[Property]
		public uint? SynonymFirmCrCode { get; set; }

		[Property]
		public uint Quantity { get; set; }

		[Property]
		public uint? RequestRatio { get; set; }

		[Property]
		public float? OrderCost { get; set; }

		[Property]
		public uint? MinOrderCount { get; set; }

		[ManyToOne(ClassType = typeof (PriceList), Lazy = Laziness.False)]
		public PriceList PriceList { get; set; }

		public bool IsOrderCostValide(uint quantity)
		{
			if (OrderCost == null)
				return true;
			if (OrderCost.Value == 0)
				return true;
			return (quantity*Cost) >= OrderCost.Value;
		}

		public bool GetRequestRationRemainder(uint quantity, out uint remainder)
		{
			remainder = 0;
			if (RequestRatio == null)
				return true;
			if (RequestRatio == 0)
				return true;
			if (quantity < RequestRatio)
				return false;
			remainder = quantity%RequestRatio.Value;
			return true;
		}

		public bool IsMinOrderCountValide(uint qunatity)
		{
			if (MinOrderCount == null)
				return true;
			return MinOrderCount <= qunatity;
		}

		public override string ToString()
		{
			return
				String.Format(
					"Id = {0}, ProductId = {1}, CodeFirmCr = {2}, Cost = {3}, Quantity = {4}, OrderCost = {5}, RequestRatio = {6}, MinOrderCount = {7}, PriceList = {8}",
					Id, ProductId, CodeFirmCr, Cost, Quantity, OrderCost, RequestRatio, MinOrderCount, PriceList);
		}
	}
}