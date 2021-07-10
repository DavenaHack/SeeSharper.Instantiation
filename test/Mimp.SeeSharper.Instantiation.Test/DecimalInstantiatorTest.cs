using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class DecimalInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new DecimalInstantiator();

            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>("12345.6789"));
            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>(" 12345.6789 "));
            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>(new Dictionary<string, object?> { { "", "12345.6789" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<decimal>("abc");
            });

            Assert.IsNull(instantiator.Construct<decimal?>(""));
            Assert.IsNull(instantiator.Construct<decimal?>(null));
            Assert.IsNull(instantiator.Construct<decimal?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
