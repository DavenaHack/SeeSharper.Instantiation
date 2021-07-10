using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Test.Mock;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ConstructorMemberTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new ConstructorMemberInstantiator(new StringInstantiator());

            Assert.AreEqual(
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                },
                instantiator.Instantiate(typeof(ConstructorMemberObject), new Dictionary<string, object?>
                {
                    { nameof(ConstructorMemberObject.Prop), "prop" },
                    { nameof(ConstructorMemberObject.Bar), "bar" },
                    { nameof(ConstructorMemberObject.Num), "num" },
                }, out _));
        }


    }
}
