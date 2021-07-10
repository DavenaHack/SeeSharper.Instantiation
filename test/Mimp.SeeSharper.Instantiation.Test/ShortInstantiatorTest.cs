using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ShortInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ShortInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<short>("12345"));
            Assert.AreEqual(12345, instantiator.Construct<short>(" 12345 "));
            Assert.AreEqual(12345, instantiator.Construct<short>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<short>("abc");
            });

            Assert.IsNull(instantiator.Construct<short?>(""));
            Assert.IsNull(instantiator.Construct<short?>(null));
            Assert.IsNull(instantiator.Construct<short?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
