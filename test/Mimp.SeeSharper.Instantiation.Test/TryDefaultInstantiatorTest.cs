using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TryDefaultInstantiatorTest
    {


        [TestMethod]
        public void TestInitialize()
        {
            var instantiator = new TryDefaultInstantiator(new IInstantiator[] {
                new BooleanInstantiator()
            });

            var b = instantiator.Instantiate<bool>(ObjectDescriptions.Constant("true"), out _);
            instantiator.Initialize(b, ObjectDescriptions.NullDescription, out _);
        }


    }
}
