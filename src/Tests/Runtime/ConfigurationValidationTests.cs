using System;
using System.Linq;
using System.Collections.Generic;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Styles;

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
        public void FeatureFlag3State_InvalidExclude()
        {
            var flag = new Dotvvm3StateFeatureFlag("myFlag");
            flag.Enabled = false;
            var e = XAssert.ThrowsAny<Exception>(() => flag.ExcludeRoute("a"));
            XAssert.Equal("Cannot exclude route 'a' because the feature flag myFlag is disabled by default.", e.Message);
        }

        [TestMethod]
        public void FeatureFlag3State_ValidOperations()
        {
            var flag = new Dotvvm3StateFeatureFlag("myFlag");

            Assert.IsNull(flag.Enabled);
            Assert.IsNull(flag.IsEnabledForRoute("a"));
            Assert.IsTrue(flag.IsEnabledForRoute("a", true));

            flag.EnableForAllRoutes();
            Assert.IsTrue(flag.Enabled);
            Assert.IsTrue(flag.IsEnabledForRoute("a", false));
            XAssert.Empty(flag.IncludedRoutes);
            XAssert.Empty(flag.ExcludedRoutes);

            flag.ExcludeRoute("a");
            Assert.IsTrue(flag.Enabled);
            XAssert.Contains("a", flag.ExcludedRoutes);
            Assert.IsFalse(flag.IsEnabledForRoute("a"));
            Assert.IsTrue(flag.IsEnabledForRoute("b"));

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

            flag.Reset().IncludeRoute("always-enabled").ExcludeRoute("always-disabled");
            Assert.IsNull(flag.IsEnabledForRoute("a"));
            Assert.IsTrue(flag.IsEnabledForRoute("always-enabled"));
            Assert.IsFalse(flag.IsEnabledForRoute("always-disabled"));
        }

        [TestMethod]
        public void FeatureFlagGlobal3State_ValidOperations()
        {
            var flag = new DotvvmGlobal3StateFeatureFlag("myFlag");
            Assert.IsNull(flag.Enabled);
            flag.Enable();
            Assert.IsTrue(flag.Enabled);
            flag.Disable();
            Assert.IsFalse(flag.Enabled);
            flag.Reset();
            Assert.IsNull(flag.Enabled);
            flag.Freeze();
            XAssert.ThrowsAny<Exception>(() => flag.Enabled = true);
        }

        [TestMethod]
        public void Freezing()
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            config.Freeze();
            XAssert.ThrowsAny<Exception>(() => config.RouteTable.Add("a", "a", null, null, presenterFactory: _ => throw new NotImplementedException()));
            XAssert.ThrowsAny<Exception>(() => config.RouteTable.AddRouteRedirection("b", "url", "a"));
            XAssert.ThrowsAny<Exception>(() => config.ApplicationPhysicalPath = "a");
            XAssert.ThrowsAny<Exception>(() => config.Debug = true);
            XAssert.ThrowsAny<Exception>(() => config.DefaultCulture = "a");
            XAssert.ThrowsAny<Exception>(() => config.Diagnostics.PerfWarnings.BigViewModelBytes = 1);
            XAssert.ThrowsAny<Exception>(() => config.Diagnostics.PerfWarnings.IsEnabled = false);
            XAssert.ThrowsAny<Exception>(() => config.Diagnostics.CompilationPage.AuthorizationPredicate = _ => Task.FromResult(true));
            XAssert.ThrowsAny<Exception>(() => config.Diagnostics.CompilationPage.RouteName = "a");
            XAssert.ThrowsAny<Exception>(() => config.Resources.DefaultResourceProcessors.Add(new SpaModeResourceProcessor(config)));
            // adding resources is actually explicitly allowed as they are stored in a ConcurrentDictionary
            config.Resources.Register("my-test-resource", new InlineScriptResource("alert(1)"));
            XAssert.ThrowsAny<Exception>(() => config.RouteConstraints.Add("a", new GenericRouteParameterType(_ => "..", (_, _) => new ParameterParseResult(true, "yes"))));
            XAssert.ThrowsAny<Exception>(() => config.Markup.Assemblies.Add("aa"));
            XAssert.ThrowsAny<Exception>(() => config.Markup.ViewCompilation.CompileInParallel = false);
            XAssert.ThrowsAny<Exception>(() => config.Markup.ImportedNamespaces.Add(new("ns1")));
            XAssert.ThrowsAny<Exception>(() => config.Markup.DefaultDirectives.Add(new("import", "ABC")));
            XAssert.ThrowsAny<Exception>(() => config.Markup.HtmlAttributeTransforms.Remove(new HtmlTagAttributePair { TagName = "a", AttributeName = "href" }));
            XAssert.ThrowsAny<Exception>(() => config.Runtime.ReloadMarkupFiles.Enable());
            XAssert.ThrowsAny<Exception>(() => config.Runtime.GlobalFilters.Add(null));
            XAssert.ThrowsAny<Exception>(() => config.Security.ContentTypeOptionsHeader.IncludedRoutes.Add("abc"));
            XAssert.ThrowsAny<Exception>(() => config.Security.ContentTypeOptionsHeader.ExcludedRoutes.Add("abc"));
            XAssert.ThrowsAny<Exception>(() => config.Security.FrameOptionsCrossOrigin.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.FrameOptionsSameOrigin.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.FrameOptionsSameOrigin.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.RequireSecFetchHeaders.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.VerifySecFetchForCommands.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.XssProtectionHeader.Enabled = true);
            XAssert.ThrowsAny<Exception>(() => config.Security.ReferrerPolicyValue = "aa");
            XAssert.ThrowsAny<Exception>(() => config.Security.SessionIdCookieName = "aa");
            XAssert.ThrowsAny<Exception>(() => config.Styles.RegisterForTag("div").SetAttribute("data-a", "b"));
            XAssert.ThrowsAny<Exception>(() => config.Styles.Styles = new List<IStyle>());
            XAssert.ThrowsAny<Exception>(() => config.Styles.Styles.RemoveAt(0));
            XAssert.ThrowsAny<Exception>(() => config.Styles.Styles.Add(null));

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

        [TestMethod]
#pragma warning disable CS0618 // Type or member is obsolete
        public void ExplicitAssemblyLoading_BackwardCompatibility()
        {
            var config = DotvvmTestHelper.DefaultConfig;

            Assert.AreSame(config.Runtime.ExplicitAssemblyLoading, config.ExperimentalFeatures.ExplicitAssemblyLoading);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
