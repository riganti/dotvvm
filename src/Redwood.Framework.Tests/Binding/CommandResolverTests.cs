using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Tests.Binding
{
    [TestClass]
    public class CommandResolverTests
    {

        [TestMethod]
        public void CommandResolver_Valid_SimpleTest()
        {
            var testObject = new
            {
                A = new[]
                {
                    new TestA() { StringToPass = "test" }
                },
                NumberToPass = 16
            };
            var viewRoot = new RedwoodView() { DataContext = testObject };

            var path = new [] { "A[0]" };
            var command = "Test(StringToPass, _parent.NumberToPass)";

            var resolver = new CommandResolver();
            resolver.GetFunction(viewRoot, testObject, path, command)();

            Assert.AreEqual(testObject.NumberToPass, testObject.A[0].ResultInt);
            Assert.AreEqual(testObject.A[0].ResultString, testObject.A[0].ResultString);
        }

        [TestMethod]
        public void CommandResolver_Valid_SimpleTest2()
        {
            var testObject = new
            {
                A = new[]
                {
                    new TestA() { StringToPass = "test" }
                },
                NumberToPass = 16
            };
            var viewRoot = new RedwoodView() { DataContext = testObject };

            var path = new[] { "A[0]", "StringToPass" };
            var command = "_parent.Test(_parent.StringToPass, _root.NumberToPass)";

            var resolver = new CommandResolver();
            resolver.GetFunction(viewRoot, testObject, path, command)();

            Assert.AreEqual(testObject.NumberToPass, testObject.A[0].ResultInt);
            Assert.AreEqual(testObject.A[0].ResultString, testObject.A[0].ResultString);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void CommandResolver_CannotCallSetter()
        {
            var testObject = new TestA()
            {
                StringToPass = "a"
            };
            var viewRoot = new RedwoodView() { DataContext = testObject };

            var path = new string[] { };
            var command = "set_StringToPass(StringToPass)";

            var resolver = new CommandResolver();
            resolver.GetFunction(viewRoot, testObject, path, command)();
        }

        public class TestA
        {
            public string StringToPass { get; set; }

            public string ResultString { get; set; }

            public int ResultInt { get; set; }

            public void Test(string s, int i)
            {
                ResultString = s;
                ResultInt = i;
            }
        }
    }
}
