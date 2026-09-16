using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class FirstOrDefaultTests : AppSeleniumTest
    {
        public FirstOrDefaultTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData("predicate")]
        [InlineData("where")]
        public void Feature_FirstOrDefault_NestedRepeaters(string variant)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_FirstOrDefault + "?variant=" + variant);
                browser.WaitUntilDotvvmInited();

                var cells = browser.FindElements("[data-ui=products] tbody td").ToArray();
                Assert.Equal(9, cells.Length);
                var expected = new[] { "Red", "", "", "", "Green,Yellow", "", "", "", "Orange" };
                for (var i = 0; i < expected.Length; i++)
                {
                    AssertUI.TextEquals(cells[i], expected[i]);
                }
                Assert.True((bool)browser.GetJavaScriptExecutor().ExecuteScript(
                    "return dotvvm.viewModels.root.viewModel.Products().every(ko.isObservable);"));
            });
        }

        [Theory]
        [InlineData("predicate", "name", "Name")]
        [InlineData("predicate", "", "")]
        [InlineData("where", "name", "Name")]
        [InlineData("where", "", "")]
        public void Feature_FirstOrDefault_MarkupControl(string variant, string key, string expected)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_FirstOrDefaultMarkupControl + "?variant=" + variant + "&key=" + key);
                browser.WaitUntilDotvvmInited();

                AssertUI.TextEquals(browser.First("[data-ui=first]"), "Name");
                AssertUI.TextEquals(browser.First("[data-ui=true]"), "Name");
                AssertUI.TextEquals(browser.First("[data-ui=lookup]"), expected);
                Assert.True((bool)browser.GetJavaScriptExecutor().ExecuteScript(
                    "return ko.contextFor(document.querySelector('[data-ui=first]')).$control.DostupnaRazeni().every(ko.isObservable);"));

                browser.First("[data-ui=key]").Clear().SendKeys("date").SendKeys(Keys.Tab);
                AssertUI.TextEquals(browser.First("[data-ui=lookup]"), "Date");

                browser.First("[data-ui=clear]").Click();
                AssertUI.TextEquals(browser.First("[data-ui=first]"), "");
                AssertUI.TextEquals(browser.First("[data-ui=true]"), "");
                AssertUI.TextEquals(browser.First("[data-ui=lookup]"), "");
            });
        }
    }
}
