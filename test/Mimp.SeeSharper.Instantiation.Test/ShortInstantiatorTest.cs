using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ShortInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ShortInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<short>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual(12345, instantiator.Construct<short>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual(12345, instantiator.Construct<short>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<short>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<short?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<short?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<short?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
