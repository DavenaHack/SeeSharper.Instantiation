using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class FloatInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new FloatInstantiator();

            Assert.AreEqual(12345.6789f, instantiator.Construct<float>(ObjectDescriptions.Constant("12345.6789")));
            Assert.AreEqual(12345.6789f, instantiator.Construct<float>(ObjectDescriptions.Constant(" 12345.6789 ")));
            Assert.AreEqual(12345.6789f, instantiator.Construct<float>(ObjectDescriptions.Constant("12345.6789").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<float>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<float?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<float?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<float?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
