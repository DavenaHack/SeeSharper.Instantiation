using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Instantiation.Type;
using Mimp.SeeSharper.TypeResolver;
using System.Collections.Generic;
using SysType = System.Type;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TypeInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new TypeInstantiator(new DelegateTypeResolver());

            Assert.AreEqual(typeof(string), instantiator.Construct<SysType>(typeof(string).FullName));
            Assert.AreEqual(typeof(string), instantiator.Construct<SysType>(new Dictionary<string, object?> { { "", typeof(string).FullName } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<SysType>("abc");
            });
        }


    }
}
