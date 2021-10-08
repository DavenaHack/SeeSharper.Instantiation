using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Instantiation.TypeResolver;
using Mimp.SeeSharper.ObjectDescription;
using Mimp.SeeSharper.TypeResolver;
using System;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TypeInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ResolveTypeInstantiator(new DelegateTypeResolver());

            Assert.AreEqual(typeof(string), instantiator.Construct<Type>(ObjectDescriptions.Constant(typeof(string).FullName)));
            Assert.AreEqual(typeof(string), instantiator.Construct<Type>(ObjectDescriptions.Constant(typeof(string).FullName).WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<Type>(ObjectDescriptions.Constant("abc"));
            });
        }


    }
}
