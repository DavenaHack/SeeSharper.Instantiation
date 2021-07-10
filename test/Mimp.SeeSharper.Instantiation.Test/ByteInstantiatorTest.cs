using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ByteInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ByteInstantiator();

            Assert.AreEqual(0, instantiator.Construct<byte>("0"));
            Assert.AreEqual(0, instantiator.Construct<byte>(" 0 "));
            Assert.AreEqual(0, instantiator.Construct<byte>(new Dictionary<string, object?> { { "", "0" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<byte>("12345");
            });

            Assert.IsNull(instantiator.Construct<byte?>(""));
            Assert.IsNull(instantiator.Construct<byte?>(null));
            Assert.IsNull(instantiator.Construct<byte?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
