using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class AttributeTests : AppSeleniumTest
    {
        public AttributeTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_Attribute_ToStringConversion()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Attribute_ToStringConversion);

                // numbers
                AssertUI.Attribute(browser.ElementAt("input[type=range]", 0), "value", a => a == "45.3");
                AssertUI.HasAttribute(browser.ElementAt("input[type=range]", 0), "data-bind");

                AssertUI.Attribute(browser.ElementAt("input[type=range]", 1), "value", a => a == "15");
                AssertUI.HasNotAttribute(browser.ElementAt("input[type=range]", 1), "data-bind");

                AssertUI.Attribute(browser.ElementAt("input[type=range]", 2), "value", a => a == "30");
                AssertUI.HasNotAttribute(browser.ElementAt("input[type=range]", 2), "data-bind");

                // bool
                AssertUI.HasNotAttribute(browser.ElementAt("details", 0), "open");
                AssertUI.HasAttribute(browser.ElementAt("details", 0), "data-bind");

                AssertUI.HasAttribute(browser.ElementAt("details", 1), "open");
                AssertUI.HasNotAttribute(browser.ElementAt("details", 1), "data-bind");

                AssertUI.HasNotAttribute(browser.ElementAt("details", 2), "open");
                AssertUI.HasNotAttribute(browser.ElementAt("details", 2), "data-bind");

                browser.Single("input[type=checkbox]").Click();
                AssertUI.HasAttribute(browser.ElementAt("details", 0), "open");

                browser.Single("input[type=checkbox]").Click();
                AssertUI.HasNotAttribute(browser.ElementAt("details", 0), "open");
            });
        }
    }
}
