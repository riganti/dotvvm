using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class RenderSettingsModeServerTest : AppSeleniumTest
    {
        [Fact]
        public void Feature_RenderSettingsModeServer_RenderSettingModeServerProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServer_RenderSettingModeServerProperty);

                // ensure month names are rendered on the server
                browser.FindElements("table tr td span").ThrowIfDifferentCountThan(12);

                // fill textboxes
                browser.SendKeys("input[type=text]", "1");

                browser.Click("input[type=button]");

                // validate result
                AssertUI.InnerTextEquals(browser.Last("span"), "12", false, true);
            });
        }

        [Fact]
        public void Feature_RenderSettingsModeServer_RepeaterCollectionExchange()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServer_RepeaterCollectionExchange);

                var repeater = browser.Single(".repeater");
                var gridView = browser.Single(".gridview");
                var comboBox = browser.Single(".combobox");

                repeater.FindElements("li").ThrowIfDifferentCountThan(2);
                gridView.FindElements("tbody tr").ThrowIfDifferentCountThan(2);
                comboBox.FindElements("option").ThrowIfDifferentCountThan(2);

                // use null
                browser.ElementAt("input[type=checkbox]", 0).Click();
                repeater.FindElements("li").ThrowIfDifferentCountThan(0);
                browser.FindElements(".gridview").ThrowIfDifferentCountThan(0);
                comboBox.FindElements("option").ThrowIfDifferentCountThan(0);

                // don't use null
                browser.ElementAt("input[type=checkbox]", 0).Click();
                gridView = browser.Single(".gridview");
                repeater.FindElements("li").ThrowIfDifferentCountThan(2);
                gridView.FindElements("tbody tr").ThrowIfDifferentCountThan(2);
                comboBox.FindElements("option").ThrowIfDifferentCountThan(2);

                // use alternative collection
                browser.ElementAt("input[type=checkbox]", 1).Click();
                repeater.FindElements("li").ThrowIfDifferentCountThan(2);
                gridView.FindElements("tbody tr").ThrowIfDifferentCountThan(2);
                comboBox.FindElements("option").ThrowIfDifferentCountThan(2);
                AssertUI.InnerTextEquals(repeater.First("li"), "alternative item 1");
                AssertUI.InnerTextEquals(gridView.First("tbody tr"), "alternative item 1");
                AssertUI.InnerTextEquals(comboBox.First("option"), "alternative item 1");
            });
        }

        [Fact]
        public void Feature_RenderSettingsModeServer_RepeaterCollectionSetToNull()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_RenderSettingsModeServer_RepeaterCollectionSetToNull);

                var value = browser.Single(".value");
                AssertUI.InnerTextEquals(value, "");
                browser.FindElements("ul li").ThrowIfDifferentCountThan(2);

                for (var i = 0; i < 2; i++)
                {
                    browser.ElementAt("input[type=button]", 0).Click();
                    AssertUI.InnerTextEquals(value, "Null assigned to the collection");
                    browser.FindElements("ul li").ThrowIfDifferentCountThan(0);

                    browser.ElementAt("input[type=button]", 1).Click();
                    AssertUI.InnerTextEquals(value, "Non-null assigned to the collection");
                    browser.FindElements("ul li").ThrowIfDifferentCountThan(2);
                }
            });
        }

        public RenderSettingsModeServerTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
