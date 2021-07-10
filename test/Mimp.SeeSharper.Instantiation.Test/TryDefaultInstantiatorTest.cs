using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;

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

            var b = instantiator.Instantiate(typeof(bool), "true", out _);
            instantiator.Initialize(b, null, out _);
        }


    }
}
