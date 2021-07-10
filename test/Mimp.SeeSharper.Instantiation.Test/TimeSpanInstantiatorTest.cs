using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class TimeSpanInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instantiator = new TimeSpanInstantiator();

            var timespan = new TimeSpan(1, 2, 3, 4, 5);
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(timespan.ToString()));
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(timespan.ToString()));
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(new Dictionary<string, object?> { { "", timespan.ToString() } }));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<TimeSpan>("abc");
            });

            Assert.IsNull(instantiator.Construct<TimeSpan?>(""));
            Assert.IsNull(instantiator.Construct<TimeSpan?>(null));
            Assert.IsNull(instantiator.Construct<TimeSpan?>(new Dictionary<string, object?> { { "", "" } }));
        }


    }
}
