using System;
using System.ServiceModel.Description;
using InforoomOnline.Tests.Properties;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class IntegrationFixture
    {
        [Test]
        public void IntegrationTest()
        {
            var service = new localhost.InforoomOnlineService();
            var offers = service.GetOffers(null, null, false, true, null, null, 1000, true, 0, true);
            Assert.That(offers.Tables.Count,
                        Is.EqualTo(1));
        }

		[Test]
		public void CheckWsdl()
		{
			var metaTransfer =
				new MetadataExchangeClient(new Uri(Settings.Default.InforoomOnline_Tests_localhost_InforoomOnlineService + "?wsdl"),
				                           MetadataExchangeClientMode.HttpGet);
			metaTransfer.ResolveMetadataReferences = true;
			var otherDocs = metaTransfer.GetMetadata();
			Assert.That(otherDocs.MetadataSections.Count, Is.GreaterThan(0));
		}
    }
}
