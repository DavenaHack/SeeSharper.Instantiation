using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Test.Mock;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ConstructorMemberInstantiatorTest
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
                instantiator.Instantiate(typeof(ConstructorMemberObject), ObjectDescriptions.EmptyDescription
                    .Append(nameof(ConstructorMemberObject.Prop), "prop")
                    .Append(nameof(ConstructorMemberObject.Bar), "bar")
                    .Append(nameof(ConstructorMemberObject.Num), "num").Constant(), out _));
        }


    }
}
