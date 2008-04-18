using System;
using log4net.Core;
using log4net.Filter;

namespace InforoomOnline.Logging
{
	public class DeadLockExceptionFilter : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			throw new NotImplementedException();
		}
	}
}
