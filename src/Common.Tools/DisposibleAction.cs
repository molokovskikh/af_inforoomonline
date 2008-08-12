using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
	public delegate void Action();

	public class DisposibleAction<T> : IDisposable
	{
		private readonly Action _action;
		private readonly T _value;

		public DisposibleAction(Action action)
		{
			_action = action;
		}

		public DisposibleAction(Action action, T value)
		{
			_action = action;
			_value = value;
		}

		public T Value
		{
			get { return _value; }
		}

		public void Dispose()
		{
			_action();
		}
	}

	public class DisposibleAction : DisposibleAction<object>
	{
		public DisposibleAction(Action action) : base(action)
		{}
	}
}
