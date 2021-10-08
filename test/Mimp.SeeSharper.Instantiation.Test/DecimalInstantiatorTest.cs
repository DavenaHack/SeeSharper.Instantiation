using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class DecimalInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new DecimalInstantiator();

            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>(ObjectDescriptions.Constant("12345.6789")));
            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>(ObjectDescriptions.Constant(" 12345.6789 ")));
            Assert.AreEqual(12345.6789m, instantiator.Construct<decimal>(ObjectDescriptions.Constant("12345.6789").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<decimal>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<decimal?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<decimal?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<decimal?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
