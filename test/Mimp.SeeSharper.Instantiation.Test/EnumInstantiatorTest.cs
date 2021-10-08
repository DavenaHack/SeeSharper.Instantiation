using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using System.Globalization;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class EnumInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new EnumInstantiator();

            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($"{nameof(NumberStyles.Integer)}")));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($"{(int)NumberStyles.Integer}")));
            Assert.AreEqual(NumberStyles.Integer | NumberStyles.AllowCurrencySymbol, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($"{nameof(NumberStyles.Integer)},{nameof(NumberStyles.AllowCurrencySymbol)}")));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($"{nameof(NumberStyles.Integer)}")));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($" {nameof(NumberStyles.Integer)} ")));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant($"{nameof(NumberStyles.Integer)}").WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<NumberStyles>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<NumberStyles?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<NumberStyles?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<NumberStyles?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
