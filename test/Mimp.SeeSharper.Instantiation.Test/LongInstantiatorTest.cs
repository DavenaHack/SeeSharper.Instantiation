using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class LongInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new LongInstantiator();

            Assert.AreEqual(12345, instantiator.Construct<long>(ObjectDescriptions.Constant("12345")));
            Assert.AreEqual(12345, instantiator.Construct<long>(ObjectDescriptions.Constant(" 12345 ")));
            Assert.AreEqual(12345, instantiator.Construct<long>(ObjectDescriptions.Constant("12345").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<long>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<long?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<long?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<long?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
