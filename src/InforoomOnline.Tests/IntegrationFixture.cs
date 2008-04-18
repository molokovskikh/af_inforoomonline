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
    }
}
