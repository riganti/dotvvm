using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ServerCommentsTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ServerComments_ServerComments()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerComments_ServerComments);

                AssertUI.InnerText(browser.Single("#before"), s => s.Contains("Before Server"));
                AssertUI.InnerText(browser.Single("#afterFirst"), s => s.Contains("After Server"));
                AssertUI.InnerText(browser.Single("#afterOther"), s => s.Contains("After Other"));
                browser.FindElements("#firstHidden").ThrowIfDifferentCountThan(0);
                browser.FindElements("#otherHidden").ThrowIfDifferentCountThan(0);
            });
        }

        public ServerCommentsTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
