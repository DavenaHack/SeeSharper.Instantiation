using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using System;
using System.Globalization;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class DateTimeInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new DateTimeInstantiator();

            var datetime = DateTime.Now;
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(ObjectDescriptions.Constant(datetime.ToString("O", CultureInfo.InvariantCulture))));
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(ObjectDescriptions.Constant(datetime.ToString("O", CultureInfo.InvariantCulture))));
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(ObjectDescriptions.Constant(datetime.ToString("O", CultureInfo.InvariantCulture)).WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<DateTime>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<DateTime?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<DateTime?>(ObjectDescriptions.Constant("0000-00-00T00:00:00.000")));
            Assert.IsNull(instantiator.Construct<DateTime?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<DateTime?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
