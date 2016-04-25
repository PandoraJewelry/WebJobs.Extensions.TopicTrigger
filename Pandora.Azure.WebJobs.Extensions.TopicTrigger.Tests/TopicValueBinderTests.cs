// Copyright (c) PandoraJewelry. All rights reserved.
// Licensed under the MIT License. See License in the project root for license information.

using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Azure.WebJobs.Extensions.TopicTrigger.Binding;
using Pandora.ServiceBus;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pandora.Azure.WebJobs.Extensions.TopicTrigger.Tests
{
    [TestClass]
    public class TopicValueBinderTests
    {
        #region ctors
        [TestMethod]
        public void CtorBasicFlow()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooObject").GetParameters();
            var msg = new BrokeredMessage();

            new TopicValueBinder(msg, info[0]);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorNullMessage()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooObject").GetParameters();

            new TopicValueBinder(null, info[0]);
        }
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void CtorNullParameterInfo()
        {
            var msg = new BrokeredMessage();

            new TopicValueBinder(msg, null);
        }
        #endregion

        #region GetValue
        [TestMethod]
        public void GetValueBrokeredMessage()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooBrokeredMessage").GetParameters();
            var msg = new BrokeredMessage() { MessageId = Guid.NewGuid().ToString() };
            var binder = new TopicValueBinder(msg, info[0]);

            var value = binder.GetValue() as BrokeredMessage;

            Assert.IsNotNull(value);
            Assert.AreEqual(msg.MessageId, value.MessageId);
        }
        [TestMethod]
        public async Task GetValueStream()
        {
            using (var stream = new MemoryStream())
            {
                var expected = new byte[] { 1, 2, 3, 4, 5 };
                await stream.WriteAsync(expected, 0, 5);
                await stream.FlushAsync();
                stream.Position = 0;

                var info = typeof(TopicValueBinderTests).GetMethod("FooStream").GetParameters();
                var msg = new BrokeredMessage(stream, false) { MessageId = Guid.NewGuid().ToString() };
                var binder = new TopicValueBinder(msg, info[0]);

                using (var value = binder.GetValue() as Stream)
                {
                    Assert.IsNotNull(value);

                    var actual = new byte[10];
                    var len = await value.ReadAsync(actual, 0, actual.Length);

                    Assert.AreEqual(5, len);
                    for (int i = 0; i < expected.Length; i++)
                        Assert.AreEqual(expected[i], actual[i]);
                }
            }
        }
        [TestMethod]
        public async Task GetValueJsonPoco()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooPoco").GetParameters();
            var foo = new Foo() { A = 5, B = "xxx" };
            var msg = await foo.CreateMessageAsync();
            var binder = new TopicValueBinder(msg, info[0]);

            var value = binder.GetValue() as Foo;

            Assert.IsNotNull(value);
            Assert.AreEqual(foo.A, value.A);
            Assert.AreEqual(foo.B, value.B);
        }
        [TestMethod]
        public async Task GetValueBinaryPoco()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooPoco").GetParameters();
            var foo = new Foo() { A = 5, B = "xxx" };
            var msg = await foo.CreateMessageAsync(false, false);
            var binder = new TopicValueBinder(msg, info[0]);

            var value = binder.GetValue() as Foo;

            Assert.IsNotNull(value);
            Assert.AreEqual(foo.A, value.A);
            Assert.AreEqual(foo.B, value.B);
        }
        [TestMethod]
        public async Task GetValueLargeBinaryPoco()
        {
            var info = typeof(TopicValueBinderTests).GetMethod("FooPoco").GetParameters();
            var foo = new Foo() { A = 5, B = new string('x', 10000) };
            var msg = await foo.CreateMessageAsync(false, false);
            var binder = new TopicValueBinder(msg, info[0]);

            var value = binder.GetValue() as Foo;

            Assert.IsNotNull(value);
            Assert.AreEqual(foo.A, value.A);
            Assert.AreEqual(foo.B, value.B);
        }
        #endregion

        #region tools
        public void FooObject(object xxx) { }
        public void FooBrokeredMessage(BrokeredMessage xxx) { }
        public void FooStream(Stream xxx) { }
        public void FooPoco(Foo xxx) { }
        public class Foo
        {
            public int A { get; set; }
            public string B { get; set; }
        }
        #endregion
    }
}
