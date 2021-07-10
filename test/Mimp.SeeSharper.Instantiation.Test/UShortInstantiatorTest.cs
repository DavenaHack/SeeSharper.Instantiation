using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class UShortInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new UShortInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<ushort>("12345"));
            Assert.AreEqual(12345, instantiator.Construct<ushort>(" 12345 "));
            Assert.AreEqual(12345, instantiator.Construct<ushort>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<ushort>("abc");
            });

            Assert.IsNull(instantiator.Construct<ushort?>(""));
            Assert.IsNull(instantiator.Construct<ushort?>(null));
            Assert.IsNull(instantiator.Construct<ushort?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
