using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
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

            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual(new BigInteger(12345), instantiator.Construct<BigInteger>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<BigInteger>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<BigInteger?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<BigInteger?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<BigInteger?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
