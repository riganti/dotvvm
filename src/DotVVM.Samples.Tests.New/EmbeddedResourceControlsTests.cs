using DotVVM.Testing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Assert = Riganti.Utils.Testing.Selenium.Core.Assert;

namespace DotVVM.Samples.Tests.New
{
    public class EmbeddedResourceControlsTests : AppSeleniumTest
    {
        public EmbeddedResourceControlsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_EmbeddedResourceControls_EmbeddedResourceControls()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_EmbeddedResourceControls_EmbeddedResourceControls);

                Assert.CheckAttribute(browser.First("input[type=button]"), "value", "Nothing");

                browser.First("input[type=button]").Click();

                Assert.CheckAttribute(browser.First("input[type=button]"), "value", "This is text");
            });
        }
    }
}
