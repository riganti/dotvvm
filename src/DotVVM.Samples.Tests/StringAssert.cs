using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests
{
    public static class StringAssert

    {
        public static void IsNullOrWhiteSpace(string value, string message)
        {
            Assert.IsTrue(string.IsNullOrWhiteSpace(value), message);
        }

        public static void IsNotNullOrWhiteSpace(string value, string message)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(value), message);
        }
    }
}