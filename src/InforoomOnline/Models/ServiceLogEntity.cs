using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace InforoomOnline.Models
{
    [Class(Table = "Logs.OnlineServicesLogs", Lazy = false)]
    public class ServiceLogEntity
    {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public uint Id { get; set; }

        [Property]
        public string ServiceName { get; set; }

        [Property]
        public DateTime LogTime { get; set; }

        [Property]
        public string Host { get; set; }

        [Property]
        public string UserName { get; set; }

        [Property]
        public string MethodName { get; set; }

        [Property]
        public int RowCount { get; set; }

        [Property]
        public int ProcessingTime { get; set; }

        [Property]
        public string Arguments { get; set; }

        public ServiceLogEntity SerializeArguments(object[] arguments)
        {
            var serializer = new JsonSerializer();
            
            var builder = new StringBuilder();
            serializer.Converters.Add(new DateTimeConverter());
            serializer.Serialize(new StringWriter(builder), arguments);
            Arguments = builder.ToString();
            return this;
        }

        public ServiceLogEntity GetHostFromOprationContext(OperationContext context)
        {
            Host = ((RemoteEndpointMessageProperty) context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;
            return this;
        }
    }

    public class DateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (DateTime);
        }

        public override void WriteJson(JsonWriter writer, object value)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
