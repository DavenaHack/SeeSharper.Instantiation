using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Instantiation.TypeResolver;
using Mimp.SeeSharper.ObjectDescription;
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
            var instantiator = new TypedInstantiator(new StringInstantiator(), new ResolveTypeInstantiator(new DelegateTypeResolver()), "$type");

            Assert.AreEqual("abc", instantiator.Construct<object>(ObjectDescriptions.EmptyDescription
                .Append("$type", typeof(string).FullName)
                .Append("abc")));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<object>(ObjectDescriptions.Constant("abc"));
            });
        }


    }
}
