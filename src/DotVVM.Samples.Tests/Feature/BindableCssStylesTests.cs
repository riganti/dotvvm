using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class BindableCssStylesTests : AppSeleniumTest
    {
        public BindableCssStylesTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_BindableCssStyles_BindableCssStyles()
        {
            const string redColorRegex = @"rgba?\(255, 0, 0(, 1)?\)";
            const string greenColorRegex = @"rgba?\(0, 128, 0(, 1)?\)";

            // ToDo: update asserts with AssertUI when there are proper assert methods available
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_BindableCssStyles_BindableCssStyles);

                var literal = browser.Single("[data-ui='literal-font-size']");
                var fontSizeInc = browser.Single("[data-ui='font-size-inc']");
                var fontSizeDec = browser.Single("[data-ui='font-size-dec']");
                Assert.Equal("14px", literal.WebElement.GetCssValue("font-size"));
                Assert.Equal("700", literal.WebElement.GetCssValue("font-weight"));
                fontSizeInc.Click().Click();
                Assert.Equal("16px", literal.WebElement.GetCssValue("font-size"));
                fontSizeDec.Click();
                Assert.Equal("15px", literal.WebElement.GetCssValue("font-size"));
                Assert.Equal("700", literal.WebElement.GetCssValue("font-weight"));

                var div = browser.Single("[data-ui='div-width']");
                var widthInc = browser.Single("[data-ui='width-inc']");
                var widthDec = browser.Single("[data-ui='width-dec']");
                Assert.Equal("50px", div.WebElement.GetCssValue("width"));
                Assert.Equal("50px", div.WebElement.GetCssValue("height"));
                Assert.Matches(greenColorRegex, div.WebElement.GetCssValue("background-color"));
                widthInc.Click().Click();
                Assert.Equal("70px", div.WebElement.GetCssValue("width"));
                widthDec.Click();
                Assert.Equal("60px", div.WebElement.GetCssValue("width"));
                Assert.Equal("50px", div.WebElement.GetCssValue("height"));
                Assert.Matches(greenColorRegex, div.WebElement.GetCssValue("background-color"));

                var span = browser.Single("[data-ui='span-color']");
                var setColorToGreen = browser.Single("[data-ui='set-color-green']");
                var setColorToRed = browser.Single("[data-ui='set-color-red']");
                Assert.Matches(redColorRegex, span.WebElement.GetCssValue("color"));
                setColorToGreen.Click();
                Assert.Matches(greenColorRegex, span.WebElement.GetCssValue("color"));
                setColorToRed.Click();
                Assert.Matches(redColorRegex, span.WebElement.GetCssValue("color"));

                var changeCondition = browser.Single("[data-ui='change-condition']");
                AssertUI.IsDisplayed(span);
                changeCondition.Click();
                AssertUI.IsNotDisplayed(span);
                changeCondition.Click();
                AssertUI.IsDisplayed(span);
            });
        }
    }
}
