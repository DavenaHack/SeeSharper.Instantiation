using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mimp.SeeSharper.Instantiation.Test.Mock;
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
                }, (IList<ConstructorMemberObject>)instantiator.Instantiate(typeof(IList<ConstructorMemberObject>), new Dictionary<string, object?>
                {
                    { nameof(ConstructorMemberObject.Prop), "prop" },
                    { nameof(ConstructorMemberObject.Bar), "bar" },
                    { nameof(ConstructorMemberObject.Num), "num" },
                }, out _)!);


            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                }
                }, (IList<ConstructorMemberObject>)instantiator.Instantiate(typeof(IList<ConstructorMemberObject>), new Dictionary<string, object?>
                {
                    { "0", new Dictionary<string, object?> {
                        { nameof(ConstructorMemberObject.Prop), "prop" },
                        { nameof(ConstructorMemberObject.Bar), "bar" },
                        { nameof(ConstructorMemberObject.Num), "num" },
                    } }
                }, out _)!);


            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("num", "bar"),
                new ConstructorMemberObject("prop")
                }, (IList<ConstructorMemberObject>)instantiator.Instantiate(typeof(IList<ConstructorMemberObject>), new Dictionary<string, object?>
                {
                    { "0", new Dictionary<string, object?> {
                        { nameof(ConstructorMemberObject.Bar), "bar" },
                        { nameof(ConstructorMemberObject.Num), "num" },
                    } },
                    { "1", new Dictionary<string, object?> {
                        { nameof(ConstructorMemberObject.Prop), "prop" },
                    } }
                }, out _)!);


        }


        [TestMethod]
        public void TestInitialize()
        {
            var instanceInstantiator = new ConstructorMemberInstantiator(new StringInstantiator());
            var instantiator = new ListInstantiator(instanceInstantiator, instanceInstantiator);

            var list = instantiator.Instantiate(typeof(IList<ConstructorMemberObject>), new Dictionary<string, object?>
                {
                    { "0", new Dictionary<string, object?> {
                        { nameof(ConstructorMemberObject.Prop), "prop" },
                    } }
                }, out _)!;
            instantiator.Initialize(list, new Dictionary<string, object?> {
                { "0", new Dictionary<string, object?> {
                    { nameof(ConstructorMemberObject.Bar), "bar" },
                    { nameof(ConstructorMemberObject.Num), "num" },
                } },
                { "1", new Dictionary<string, object?> {
                    { nameof(ConstructorMemberObject.Bar), "bar" },
                    { nameof(ConstructorMemberObject.Num), "num" },
                } }
            }, out _);

            CheckEquals(new List<ConstructorMemberObject> {
                new ConstructorMemberObject("prop")
                {
                    Bar = "bar",
                    Num = "num"
                },
                new ConstructorMemberObject("prop")
                }, (IList<ConstructorMemberObject>)list);


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
