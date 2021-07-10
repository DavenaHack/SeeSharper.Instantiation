using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class CharInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new CharInstantiator();

            Assert.AreEqual('0', instantiator.Construct<char>("0"));
            Assert.AreEqual('0', instantiator.Construct<char>(new Dictionary<string, object?> { { "", "0" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<char>("abc");
            });

            Assert.IsNull(instantiator.Construct<char?>(""));
            Assert.IsNull(instantiator.Construct<char?>(null));
            Assert.IsNull(instantiator.Construct<char?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
