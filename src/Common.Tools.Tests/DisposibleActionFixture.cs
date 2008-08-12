using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Tools.Test
{
	[TestFixture]
	public class DisposibleActionFixture
	{
		[Test]
		public void DisposeTest()
		{
			bool isCalled = false;
			new DisposibleAction<object>(delegate
			                     	{
			                     		isCalled = true;
			                     	}, null).Dispose();
			Assert.That(isCalled);
		}

		[Test]
		public void ValueTest()
		{
			DisposibleAction<int> action 
				= new DisposibleAction<int>(delegate
						                 	{
								         	}, 1);
			Assert.That(action.Value, Is.EqualTo(1));
		}
	}
}
