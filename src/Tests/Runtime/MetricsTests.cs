using System;
using DotVVM.Framework.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class MetricsTests
    {
        [TestMethod]
        public void IntLog()
        {
            for (int i = 1; i < 100; i++)
            {
                var log = DotvvmMetrics.IntegerLog2(i);
                var expectedLog = (int)Math.Log(i, 2);
                Assert.AreEqual(expectedLog, log);
            }
        }

        [TestMethod]
        public void ExponentialBuckets()
        {
            XAssert.Equal(new double[] { 1, 2, 4, 8, 16, 32, 64 }, DotvvmMetrics.ExponentialBuckets(1, 64));
            XAssert.Equal(new double[] { 0.0078125, 0.015625, 0.03125, 0.0625, 0.125, 0.25, 0.5, 1, 2, 4, 8, 16, 32, 64 }, DotvvmMetrics.TryGetRecommendedBuckets(DotvvmMetrics.RequestDuration));

        }
    }
}
