using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class UShortInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new UShortInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<ushort>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual(12345, instantiator.Construct<ushort>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual(12345, instantiator.Construct<ushort>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<ushort>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<ushort?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<ushort?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<ushort?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
