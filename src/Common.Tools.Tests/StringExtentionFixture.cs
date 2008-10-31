using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Tools.Test
{
	[TestFixture]
	public class StringExtentionFixture
	{
		[Test]
		public void Better_syntaxis_for_format()
		{
            Assert.That("{0} {1}".Format(1, 2), Is.EqualTo("1 2"));
		}
	}
}
