using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class UIntInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new UIntInstantiator();

            Assert.AreEqual((uint)12345, instantiator.Construct<uint>("12345"));
            Assert.AreEqual((uint)12345, instantiator.Construct<uint>(" 12345 "));
            Assert.AreEqual((uint)12345, instantiator.Construct<uint>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<uint>("abc");
            });

            Assert.IsNull(instantiator.Construct<uint?>(""));
            Assert.IsNull(instantiator.Construct<uint?>(null));
            Assert.IsNull(instantiator.Construct<uint?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
