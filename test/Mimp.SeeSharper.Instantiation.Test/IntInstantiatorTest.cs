using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class IntInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new IntInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<int>("12345"));
            Assert.AreEqual(12345, instantiator.Construct<int>(" 12345 "));
            Assert.AreEqual(12345, instantiator.Construct<int>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<int>("abc");
            });

            Assert.IsNull(instantiator.Construct<int?>(""));
            Assert.IsNull(instantiator.Construct<int?>(null));
            Assert.IsNull(instantiator.Construct<int?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
