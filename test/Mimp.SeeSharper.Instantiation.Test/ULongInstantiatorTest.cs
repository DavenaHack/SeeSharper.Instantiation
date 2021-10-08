using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ULongInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ULongInstantiator();

            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual((ulong)12345, instantiator.Construct<ulong>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<ulong>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<ulong?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<ulong?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<ulong?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
