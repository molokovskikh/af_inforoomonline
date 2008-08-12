using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Helpers
{
	public class LoggingHelper
	{
		public static string[] ToArrayString<T>(ICollection<T> collection)
		{
			string[] result = new string[collection.Count];
			int index = 0;
			foreach (T t in collection)
			{
				result[index] = t.ToString();
				index++;
			}
			return result;
		}

		public static string CollectionToString<T>(ICollection<T> collection)
		{
			return String.Join(", ", ToArrayString(collection));
		}
	}
}