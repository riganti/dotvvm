using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class EnvironmentViewTests : AppSeleniumTest
    {
        [Fact]
        public void Control_EnvironmentView_EnvironmentViewTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_EnvironmentView_EnvironmentViewTest);

                AssertUI.InnerTextEquals(browser.FindElements(".result1").ThrowIfDifferentCountThan(1).First(), "Development or Production environment!");
                AssertUI.InnerTextEquals(browser.FindElements(".result2").ThrowIfDifferentCountThan(1).First(), "Not Staging environment!");
            });
        }

        public EnvironmentViewTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
