using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;
using System.Numerics;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class BigIntegerInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new BigIntegerInstantiator();

            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>("12345"));
            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>(" 12345 "));
            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>(new Dictionary<string, object?> { { "", "12345" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<BigInteger>("abc");
            });

            Assert.IsNull(instantiator.Construct<BigInteger?>(""));
            Assert.IsNull(instantiator.Construct<BigInteger?>(null));
            Assert.IsNull(instantiator.Construct<BigInteger?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
