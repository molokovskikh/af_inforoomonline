using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "UserSettings.RetClientsSet", Lazy = false)]
	public class ClientSettings
	{
		[Id(0, Name = "ClientCode")]
		[Generator(1, Class = "native")]
		public uint ClientCode { get; set; }

		[Property]
		public bool SubmitOrders { get; set; }

		[Property]
		public bool AllowSubmitOrders { get; set; }

		public bool IsSubmite
		{
			get { return SubmitOrders && AllowSubmitOrders; }
		}
	}
}