using System;
using System.Linq;
using System.Collections.Generic;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ConfigurationValidationTests
    {
        OutputChecker check = new OutputChecker("config-tests");

        public ConfigurationValidationTests()
        {
        }

        [TestMethod]
        public void FeatureFlag_ValidOperations()
        {
            var flag = new DotvvmFeatureFlag("myFlag");
            flag.EnableForAllRoutes();
            Assert.IsTrue(flag.Enabled);
            XAssert.Empty(flag.IncludedRoutes);
            XAssert.Empty(flag.ExcludedRoutes);

            flag.ExcludeRoute("a");
            Assert.IsTrue(flag.Enabled);
            XAssert.Contains("a", flag.ExcludedRoutes);
            

            flag.DisableForAllRoutes()
                .IncludeRoute("a")
                .IncludeRoute("b");
            Assert.IsFalse(flag.Enabled);
            XAssert.Contains("a", flag.IncludedRoutes);
            XAssert.Contains("b", flag.IncludedRoutes);
            XAssert.Empty(flag.ExcludedRoutes);

            flag.EnableForRoutes("x", "y");
            Assert.IsFalse(flag.Enabled);
            XAssert.Equal(new [] { "x", "y" }, flag.IncludedRoutes);
            XAssert.Empty(flag.ExcludedRoutes);
        }

        [TestMethod]
        public void FeatureFlag_InvalidInclude()
        {
            var flag = new DotvvmFeatureFlag("myFlag");
            var e = XAssert.ThrowsAny<Exception>(() => flag.EnableForAllRoutes().IncludeRoute("a"));
            XAssert.Equal("Cannot include route 'a' because the feature flag myFlag is enabled by default.", e.Message);
        }

        [TestMethod]
        public void FeatureFlag_InvalidExclude()
        {
            var flag = new DotvvmFeatureFlag("myFlag");
            var e = XAssert.ThrowsAny<Exception>(() => flag.DisableForAllRoutes().ExcludeRoute("a"));
            XAssert.Equal("Cannot exclude route 'a' because the feature flag myFlag is disabled by default.", e.Message);
        }

        [TestMethod]
        public void ValidateMissingRoutes()
        {
            var config = DotvvmTestHelper.CreateConfiguration();

            config.RouteTable.Add("a", "a", null, null, presenterFactory: _ => throw new NotImplementedException());

            config.Security.XssProtectionHeader.ExcludeRoute("A");
            config.Security.XssProtectionHeader.ExcludeRoute("b");
            config.Security.RequireSecFetchHeaders.IncludeRoute("b");
            config.Security.RequireSecFetchHeaders.IncludeRoute("c");
            config.ExperimentalFeatures.LazyCsrfToken.IncludeRoute("b");

            check.CheckException(() => config.AssertConfigurationIsValid());
        }
    }
}
