using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ByteInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ByteInstantiator();

            Assert.AreEqual(0, instantiator.Construct<byte>(ObjectDescriptions.Constant("0")));
            Assert.AreEqual(0, instantiator.Construct<byte>(ObjectDescriptions.Constant(" 0 ")));
            Assert.AreEqual(0, instantiator.Construct<byte>(ObjectDescriptions.Constant("0").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<byte>(ObjectDescriptions.Constant("12345"));
            });

            Assert.IsNull(instantiator.Construct<byte?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<byte?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<byte?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
