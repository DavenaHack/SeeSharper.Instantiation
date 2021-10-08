using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.ObjectDescription;
using System;

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
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(ObjectDescriptions.Constant(timespan.ToString())));
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(ObjectDescriptions.Constant(timespan.ToString())));
            Assert.AreEqual(timespan, instantiator.Construct<TimeSpan>(ObjectDescriptions.Constant(timespan.ToString()).WrapValue()));

            Assert.ThrowsException<InstantiationException>(() =>
            {
                instantiator.Construct<TimeSpan>(ObjectDescriptions.Constant("abc"));
            });

            Assert.IsNull(instantiator.Construct<TimeSpan?>(ObjectDescriptions.Constant("")));
            Assert.IsNull(instantiator.Construct<TimeSpan?>(ObjectDescriptions.NullDescription));
            Assert.IsNull(instantiator.Construct<TimeSpan?>(ObjectDescriptions.Constant("").WrapValue()));
        }


    }
}
