using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System.Collections.Generic;
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

            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>($"{nameof(NumberStyles.Integer)}"));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>($"{(int)NumberStyles.Integer}"));
            Assert.AreEqual(NumberStyles.Integer | NumberStyles.AllowCurrencySymbol, instantiator.Construct<NumberStyles>($"{nameof(NumberStyles.Integer)},{nameof(NumberStyles.AllowCurrencySymbol)}"));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>($"{nameof(NumberStyles.Integer)}"));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>($" {nameof(NumberStyles.Integer)} "));
            Assert.AreEqual(NumberStyles.Integer, instantiator.Construct<NumberStyles>(new Dictionary<string, object?> { { "", $"{nameof(NumberStyles.Integer)}" } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<NumberStyles>("abc");
            });

            Assert.IsNull(instantiator.Construct<NumberStyles?>(""));
            Assert.IsNull(instantiator.Construct<NumberStyles?>(null));
            Assert.IsNull(instantiator.Construct<NumberStyles?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
