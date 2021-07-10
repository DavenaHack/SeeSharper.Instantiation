using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class LongInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new LongInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<long>("12345"));
            Assert.AreEqual(12345, instantiator.Construct<long>(" 12345 "));
            Assert.AreEqual(12345, instantiator.Construct<long>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<long>("abc");
            });

            Assert.IsNull(instantiator.Construct<long?>(""));
            Assert.IsNull(instantiator.Construct<long?>(null));
            Assert.IsNull(instantiator.Construct<long?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
