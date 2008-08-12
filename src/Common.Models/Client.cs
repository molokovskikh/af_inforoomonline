using System;
using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "UserSettings.Clientsdata", Lazy = false)]
	public class Client
	{
		[Id(0, Name = "FirmCode")]
		[Generator(1, Class = "native")]
		public uint FirmCode { get; set; }

		[Property]
		public uint RegionCode { get; set; }

		public override string ToString()
		{
			return String.Format("FirmCode = {0}, RegionCode = {1}", FirmCode, RegionCode);
		}
	}
}