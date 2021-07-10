using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ULongInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ULongInstantiator();

            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>("12345"));
            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>(" 12345 "));
            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<ulong>("abc");
            });

            Assert.IsNull(instantiator.Construct<ulong?>(""));
            Assert.IsNull(instantiator.Construct<ulong?>(null));
            Assert.IsNull(instantiator.Construct<ulong?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
