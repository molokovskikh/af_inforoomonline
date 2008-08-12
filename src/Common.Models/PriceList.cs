using System;
using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "usersettings.activeprices", Lazy = false)]
	public class PriceList
	{
		[Id(0, Name = "PriceCode")]
		[Generator(1, Class = "native")]
		public uint PriceCode { get; set; }

		[Property]
		public DateTime PriceDate { get; set; }

		[Property]
		public UInt32 MinReq { get; set; }

		[Property]
		public uint FirmCategory { get; set; }

		[ManyToOne(ClassType = typeof (Firm), Lazy = Laziness.False, Column = "FirmCode")]
		public Firm Firm { get; set; }

		public override string ToString()
		{
			return String.Format("PriceCode = {0}, PriceDate = {1}, MinReq = {2}, FirmCategory = {3}",
			                     PriceCode, PriceDate, MinReq, FirmCategory);
		}
	}
}