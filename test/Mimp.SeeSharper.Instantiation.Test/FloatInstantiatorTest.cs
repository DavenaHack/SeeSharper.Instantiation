using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class FloatInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new FloatInstantiator();

            Assert.AreEqual(12345.6789f, instantiator.Construct<float>("12345.6789"));
            Assert.AreEqual(12345.6789f, instantiator.Construct<float>(" 12345.6789 "));
            Assert.AreEqual(12345.6789f, instantiator.Construct<float>(new Dictionary<string, object?> { { "", "12345.6789" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<float>("abc");
            });

            Assert.IsNull(instantiator.Construct<float?>(""));
            Assert.IsNull(instantiator.Construct<float?>(null));
            Assert.IsNull(instantiator.Construct<float?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
