using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class CharInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new CharInstantiator();

            Assert.AreEqual('0', instantiator.Construct<char>(ObjectDescriptions.Constant("0")));
            Assert.AreEqual('0', instantiator.Construct<char>(ObjectDescriptions.Constant("0").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<char>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<char?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<char?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<char?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
