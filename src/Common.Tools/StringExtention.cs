using System;

namespace Common.Tools
{
	public static class StringExtention
	{
		public static string Format(this string forFormat, params object[] args)
		{
			return String.Format(forFormat, args);
		}
	}
}
