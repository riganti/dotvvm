using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature;

public class NullHandlingTests(ITestOutputHelper output) : AppSeleniumTest(output)
{
    [Theory]
    [InlineData("resource-button")]
    [InlineData("value-button")]
    [Trait("Category", "dev-only")] // error page
    public void Feature_NullHandling_Button_Enabled(string buttonId)
    {
        RunInAllBrowsers(browser => {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_NullHandling_Button_Enabled);

            var button = browser.Single(buttonId, SelectByDataUi);
            AssertUI.IsNotEnabled(button);

            browser.Single("a").Click();

            button.Click();

            var errorPage = browser.Single("#debugWindow");
            AssertUI.IsDisplayed(errorPage);
            
            var scope = browser.GetFrameScope("#debugWindow iframe");
            AssertUI.Text(scope.Single(".summary"), t => t.Contains("Execution of '{command: Value = Value + 1}' was disallowed by '<dot:Button "));
        });
    }
}
