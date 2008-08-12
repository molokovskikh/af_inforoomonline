using System;
using System.Collections.Generic;
using System.Text;

namespace Common.MySql
{
	public delegate void Action<T>(T value, int index);

	public class Iterator
	{
		public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
		{
			int i = 0;
			foreach (T value in collection)
			{
				action(value, i);
				i++;
			}
		}
	}
}
