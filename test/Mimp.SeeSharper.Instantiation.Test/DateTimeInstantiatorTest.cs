using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System;
using System.Collections.Generic;
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
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(datetime.ToString("O", CultureInfo.InvariantCulture)));
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(datetime.ToString("O", CultureInfo.InvariantCulture)));
            Assert.AreEqual(datetime, instantiator.Construct<DateTime>(new Dictionary<string, object?> { { "", datetime.ToString("O", CultureInfo.InvariantCulture) } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<DateTime>("abc");
            });

            Assert.IsNull(instantiator.Construct<DateTime?>(""));
            Assert.IsNull(instantiator.Construct<DateTime?>("0000-00-00T00:00:00.000"));
            Assert.IsNull(instantiator.Construct<DateTime?>(null));
            Assert.IsNull(instantiator.Construct<DateTime?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
