using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Abstraction;
using Mimp.SeeSharper.Instantiation.Test.Mock;
using Mimp.SeeSharper.ObjectDescription;
using System.Collections.Generic;

namespace Mimp.SeeSharper.Instantiation.Test
{
    [TestClass]
    public class ListInstantiatorTest
    {


        [TestMethod]
        public void TestInstantiate()
        {
            var instanceInstantiator = new ConstructorMemberInstantiator(new StringInstantiator());
            var instantiator = new ListInstantiator(instanceInstantiator, instanceInstantiator);


            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                }
            }, instantiator.Instantiate<IList<ConstructorMemberObject>>(ObjectDescriptions.EmptyDescription
                    .Append(nameof(ConstructorMemberObject.Prop), "prop")
                    .Append(nameof(ConstructorMemberObject.Bar), "bar")
                    .Append(nameof(ConstructorMemberObject.Num), "num").Constant(), out _)!);


            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                }
            }, instantiator.Instantiate<IList<ConstructorMemberObject>>(ObjectDescriptions.EmptyDescription
                    .Append("0", ObjectDescriptions.EmptyDescription
                        .Append(nameof(ConstructorMemberObject.Prop), "prop")
                        .Append(nameof(ConstructorMemberObject.Bar), "bar")
                        .Append(nameof(ConstructorMemberObject.Num), "num")), out _)!);


            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("num", "bar"),
                new ConstructorMemberObject("prop")
            }, instantiator.Instantiate<IList<ConstructorMemberObject>>(ObjectDescriptions.EmptyDescription
                    .Append("0", ObjectDescriptions.EmptyDescription
                        .Append(nameof(ConstructorMemberObject.Bar), "bar")
                        .Append(nameof(ConstructorMemberObject.Num), "num"))
                    .Append("1", ObjectDescriptions.EmptyDescription
                        .Append(nameof(ConstructorMemberObject.Prop), "prop")), out _)!);


        }


        [TestMethod]
        public void TestInitialize()
        {
            var instanceInstantiator = new ConstructorMemberInstantiator(new StringInstantiator());
            var instantiator = new ListInstantiator(instanceInstantiator, instanceInstantiator);

            var list = instantiator.Instantiate<IList<ConstructorMemberObject>>(ObjectDescriptions.EmptyDescription
                .Append("0", ObjectDescriptions.EmptyDescription
                    .Append(nameof(ConstructorMemberObject.Prop), "prop")), out _)!;
            instantiator.Initialize(list, ObjectDescriptions.EmptyDescription
                .Append("0", ObjectDescriptions.EmptyDescription
                    .Append(nameof(ConstructorMemberObject.Bar), "bar")
                    .Append(nameof(ConstructorMemberObject.Num), "num"))
                .Append("1", ObjectDescriptions.EmptyDescription
                    .Append(nameof(ConstructorMemberObject.Bar), "bar")
                    .Append(nameof(ConstructorMemberObject.Num), "num")), out _);

            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                },
                new ConstructorMemberObject("prop")
            }, list);


        }


        private void CheckEquals(IList<ConstructorMemberObject> expect, IList<ConstructorMemberObject> actual)
        {
            var exEn = expect.GetEnumerator();
            var acEn = actual.GetEnumerator();

            while (exEn.MoveNext() && acEn.MoveNext())
                Assert.AreEqual(exEn.Current, acEn.Current);

            Assert.IsFalse(exEn.MoveNext());
            Assert.IsFalse(acEn.MoveNext());
        }


    }
}
