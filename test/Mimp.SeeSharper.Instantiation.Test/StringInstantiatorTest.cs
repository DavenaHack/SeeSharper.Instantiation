using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class StringInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new StringInstantiator();

            Assert.AreEqual("abc", instantiator.Construct<string>(ObjectDescriptions.Constant("abc")));
            Assert.AreEqual("abc", instantiator.Construct<string>(ObjectDescriptions.Constant("abc").WrapValue()));
            Assert.AreEqual(null, instantiator.Construct<string>(ObjectDescriptions.EmptyDescription));
        }


    }
}
