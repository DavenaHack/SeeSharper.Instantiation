using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class SByteInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new SByteInstantiator();

            Assert.AreEqual(0, instantiator.Construct<sbyte>("0"));
            Assert.AreEqual(0, instantiator.Construct<sbyte>(" 0 "));
            Assert.AreEqual(0, instantiator.Construct<sbyte>(new Dictionary<string, object?> { { "", "0" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<sbyte>("12345");
            });

            Assert.IsNull(instantiator.Construct<sbyte?>(""));
            Assert.IsNull(instantiator.Construct<sbyte?>(null));
            Assert.IsNull(instantiator.Construct<sbyte?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
