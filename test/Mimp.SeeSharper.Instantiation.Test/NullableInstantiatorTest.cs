using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Test.Mock;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.Reflection;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class NullableInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new NullableInstantiator(new ThrowInstantiator());

            Assert.AreEqual(typeof(KeyValuePair<string, object>?).Default(), instantiator.Construct<KeyValuePair<string, object>?>(ObjectDescriptions.Constant("")));
            Assert.AreEqual(typeof(KeyValuePair<string, object>?).Default(), instantiator.Construct<KeyValuePair<string, object>?>(ObjectDescriptions.NullDescription));
            Assert.AreEqual(typeof(KeyValuePair<string, object>?).Default(), instantiator.Construct<KeyValuePair<string, object>?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
