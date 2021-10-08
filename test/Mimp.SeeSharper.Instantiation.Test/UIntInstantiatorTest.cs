using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class UIntInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new UIntInstantiator();

            Assert.AreEqual((uint)12345, instantiator.Construct<uint>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual((uint)12345, instantiator.Construct<uint>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual((uint)12345, instantiator.Construct<uint>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<uint>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<uint?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<uint?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<uint?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
