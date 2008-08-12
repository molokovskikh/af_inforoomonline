using NHibernate.Mapping.Attributes;

namespace Common.Models
{
	[Class(Table = "logs.SmartOrderLogs")]
	public class SmartOrderLogEntity
	{
		protected SmartOrderLogEntity()
		{}

		public SmartOrderLogEntity(uint sourceOrderLineId, OrderItem resultOrderLine, string comment)
		{
			SourceOrderLineId = sourceOrderLineId;
			ResultOrderLine = resultOrderLine;
			Comment = comment;
		}

		[Id(0, Name = "Id")]
		[Generator(1, Class = "native")]
		public virtual uint Id { get; protected set; }

		[Property]
		public virtual uint SourceOrderLineId { get; protected set; }

		[ManyToOne(ClassType = typeof(OrderItem), Column = "ResultOrderLineId")]
		public virtual OrderItem ResultOrderLine { get; protected set; }

		[Property]
		public virtual string Comment { get; set; }
	}
}
