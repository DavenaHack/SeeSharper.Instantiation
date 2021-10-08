using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class BooleanInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new BooleanInstantiator();

            Assert.AreEqual(false, instantiator.Construct<bool>(ObjectDescriptions.Constant("false")));
            Assert.AreEqual(false, instantiator.Construct<bool>(ObjectDescriptions.Constant(" false ")));
            Assert.AreEqual(false, instantiator.Construct<bool>(ObjectDescriptions.Constant("false").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<bool>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<bool?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<bool?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<bool?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
