using System;
using System.Collections.Generic;
using System.Text;

namespace InforoomOnline.Models
{
	public class ServiceLogEntity
	{
		private uint _id;
		private string _serviceName;
		private DateTime _logTime;
		private string _host;
		private string _userName;
		private string _methodName;
		private int _rowCount;
		private int _processingTime;
		private string _arguments;

		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public string ServiceName
		{
			get { return _serviceName; }
			set { _serviceName = value; }
		}

		public DateTime LogTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		public string Host
		{
			get { return _host; }
			set { _host = value; }
		}

		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		public string MethodName
		{
			get { return _methodName; }
			set { _methodName = value; }
		}

		public int RowCount
		{
			get { return _rowCount; }
			set { _rowCount = value; }
		}

		public int ProcessingTime
		{
			get { return _processingTime; }
			set { _processingTime = value; }
		}

		public string Arguments
		{
			get { return _arguments; }
			set { _arguments = value; }
		}
	}
}
