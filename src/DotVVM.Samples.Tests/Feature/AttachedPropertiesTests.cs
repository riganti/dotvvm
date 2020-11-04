using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class AttachedPropertiesTests : AppSeleniumTest
    {
        public AttachedPropertiesTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_AttachedProperties_AttachedProperties()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_AttachedProperties_AttachedProperties);

                var div = browser.Single(".div");
                AssertUI.Attribute(div, "bind:value", a => a == "test");
                AssertUI.Attribute(div, "bind:value2", a => a == "aaa");

                var txb = browser.Single("input[type=text]");
                txb.Clear().SendKeys("bbb").SendKeys(Keys.Tab);

                AssertUI.Attribute(div, "bind:value2", a => a == "bbb");
            });
        }
    }
}
