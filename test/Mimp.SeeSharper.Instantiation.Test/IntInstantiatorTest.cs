using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class IntInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new IntInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<int>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual(12345, instantiator.Construct<int>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual(12345, instantiator.Construct<int>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<int>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<int?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<int?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<int?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
