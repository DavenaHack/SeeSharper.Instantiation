using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Instantiation.Type;
using Mimp.SeeSharper.TypeResolver;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TypedInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new TypedInstantiator(new StringInstantiator(), new TypeInstantiator(new DelegateTypeResolver()), "$type");

            Assert.AreEqual("abc", instantiator.Construct<object>(new Dictionary<string, object?> {
                { "$type", typeof(string).FullName },
                { "", "abc" }
            }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<object>("abc");
            });
        }


    }
}
