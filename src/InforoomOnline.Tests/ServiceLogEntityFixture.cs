using System;
using System.Collections.Generic;
using System.Text;
using InforoomOnline.Models;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace InforoomOnline.Tests
{
    [TestFixture]
    public class ServiceLogEntityFixture
    {
        [Test]
        public void SerializeEmpty()
        {
            var entity = new ServiceLogEntity();
            entity.SerializeArguments(new object[0]);
            Assert.That(entity.Arguments, Is.EqualTo("[]"));
        }

        [Test]
        public void SerializeArgumets()
        {
            var entity = new ServiceLogEntity();
            entity.SerializeArguments(new object[] { 1, "123" });
            Assert.That(entity.Arguments, Is.EqualTo("[1,\"123\"]"));
        }

        [Test]
        public void SerializeArray()
        {
            var entity = new ServiceLogEntity();
            entity.SerializeArguments(new object[] { 1 , new[] { 1, 2, 3 }});
            Assert.That(entity.Arguments, Is.EqualTo("[1,[1,2,3]]"));
        }

        [Test]
        public void SerializeDateTimeTest()
        {
            var entity = new ServiceLogEntity();
            entity.SerializeArguments(new object[] { new DateTime(2008, 4, 17, 11, 6, 15) });
            Assert.That(entity.Arguments, Is.EqualTo("[\"17.04.2008 11:06:15\"]"));
        }

        [Test]
        public void SerializeNull()
        {
            var entity = new ServiceLogEntity();
            entity.SerializeArguments(new object[] { 1, null });
            Assert.That(entity.Arguments, Is.EqualTo("[1,null]"));
        }
    }
}
