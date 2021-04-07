using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Complex
{
    public class CascadeSelectorsTests : AppSeleniumTest
    {
        public CascadeSelectorsTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Complex_CascadeSelectors_CascadeSelectors()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectors);
        }

        [Fact]
        public void Complex_CascadeSelectors_CascadeSelectorsServerRender()
        {
            Complex_CascadeSelectorsBase(SamplesRouteUrls.ComplexSamples_CascadeSelectors_CascadeSelectorsServerRender);
        }

        [Fact]
        public void Complex_CascadeSelectors_TripleComboBoxes()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_CascadeSelectors_TripleComboBoxes);
                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 0), "North America: 1");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 1), "USA: 11");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 2), "New York: 111");

                browser.ElementAt("input[type=button]", 2).Click();
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 0), "North America: 1");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 1), "Canada: 12");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 2), "Toronto: 121");

                browser.ElementAt("input[type=button]", 5).Click();
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 0), "Europe: 2");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 1), "Germany: 21");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 2), "Munich: 212");

                browser.ElementAt("input[type=button]", 8).Click();
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 0), "Asia: 3");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 1), "China: 31");
                AssertUI.InnerTextEquals(browser.ElementAt(".active", 2), "Beijing: 311");
            });
        }

        private void Complex_CascadeSelectorsBase(string url)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(url);

                // select city
                browser.First("select").Select(1);
                browser.First("input[type=button]").Click();

                // select hotel
                browser.Last("select").Select(1);
                browser.Last("input[type=button]").Click();

                AssertUI.InnerTextEquals(browser.First("h2"), "Hotel Seattle #2");

                // select city
                browser.First("select").Select(0);
                browser.First("input[type=button]").Click();

                // select hotel
                browser.Last("select").Select(0);
                browser.Last("input[type=button]").Click();

                AssertUI.InnerTextEquals(browser.First("h2"), "Hotel Prague #1");
            });
        }
    }
}
