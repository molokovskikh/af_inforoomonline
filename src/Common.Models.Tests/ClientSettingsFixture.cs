using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Common.Models.Tests
{
	[TestFixture]
	public class ClientSettingsFixture
	{
		[Test]
		public void IsSubmiteTest()
		{
			var clientSettings = new ClientSettings();
			Assert.That(clientSettings.IsSubmite, Is.False);
			clientSettings.SubmitOrders = true;
			Assert.That(clientSettings.IsSubmite, Is.False);
			clientSettings.AllowSubmitOrders = true;
			Assert.That(clientSettings.IsSubmite, Is.True);
		}
	}
}