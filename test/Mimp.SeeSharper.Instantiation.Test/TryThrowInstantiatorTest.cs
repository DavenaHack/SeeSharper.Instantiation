using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TryThrowInstantiatorTest
    {


        [TestMethod]
        public void TestInitialize()
        {
            var instantiator = new TryThrowInstantiator(new IInstantiator[] {
                new BooleanInstantiator()
            });

            var b = instantiator.Instantiate(typeof(bool), "true", out _);
            instantiator.Initialize(b, null, out _);
        }


    }
}
