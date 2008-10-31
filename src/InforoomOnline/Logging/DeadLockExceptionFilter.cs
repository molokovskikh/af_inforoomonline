using Common.MySql;
using log4net.Core;
using log4net.Filter;

namespace InforoomOnline.Logging
{
	public class DeadLockExceptionFilter : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Error
                && ExceptionHelper.IsDeadLockOrSimilarExceptionInChain(loggingEvent.ExceptionObject))
					return FilterDecision.Deny;

			return FilterDecision.Neutral;
		}
	}
}
