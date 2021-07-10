using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class BooleanInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new BooleanInstantiator();

            Assert.AreEqual(false, instantiator.Construct<bool>("false"));
            Assert.AreEqual(false, instantiator.Construct<bool>(" false "));
            Assert.AreEqual(false, instantiator.Construct<bool>(new Dictionary<string, object?> { { "", "false" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<bool>("abc");
            });

            Assert.IsNull(instantiator.Construct<bool?>(""));
            Assert.IsNull(instantiator.Construct<bool?>(null));
            Assert.IsNull(instantiator.Construct<bool?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
