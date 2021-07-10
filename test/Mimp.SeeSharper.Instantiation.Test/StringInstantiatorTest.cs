using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class StringInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new StringInstantiator();

            Assert.AreEqual("abc", instantiator.Construct<string>("abc"));
            Assert.AreEqual("abc", instantiator.Construct<string>(new Dictionary<string, object?> { { "", "abc" } }));
            Assert.AreEqual(null, instantiator.Construct<string>(new Dictionary<string, object?>()));
        }


    }
}
