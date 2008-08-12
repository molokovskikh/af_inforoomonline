using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "UserSettings.Clientsdata", Lazy = false)]
	public class Firm
	{
		[Id(0, Name = "FirmCode")]
		[Generator(1, Class = "native")]
		public uint FirmCode { get; set; }

		[Property]
		public string ShortName { get; set; }
	}
}