
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ServerCommentsTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_ServerComments_ServerComments()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ServerComments_ServerComments);

                browser.Single("#before").CheckIfInnerText(s => s.Contains("Before Server"));
                browser.Single("#afterFirst").CheckIfInnerText(s => s.Contains("After Server"));
                browser.Single("#afterOther").CheckIfInnerText(s => s.Contains("After Other"));
                browser.FindElements("#firstHidden").ThrowIfDifferentCountThan(0);
                browser.FindElements("#otherHidden").ThrowIfDifferentCountThan(0);
            });
        }
    }
}
