using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class SByteInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new SByteInstantiator();

            Assert.AreEqual(0, instantiator.Construct<sbyte>(ObjectDescriptions.Constant("0")));
            Assert.AreEqual(0, instantiator.Construct<sbyte>(ObjectDescriptions.Constant(" 0 ")));
            Assert.AreEqual(0, instantiator.Construct<sbyte>(ObjectDescriptions.Constant("0").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<sbyte>(ObjectDescriptions.Constant("12345"));
            });

            Assert.IsNull(instantiator.Construct<sbyte?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<sbyte?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<sbyte?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
