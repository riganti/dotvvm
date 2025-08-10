using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature;

public class CsrfTokenTests(ITestOutputHelper output) : AppSeleniumTest(output)
{
    [Fact]
    public void Feature_CsrfToken_InvalidateToken()
    {
        RunInAllBrowsers(browser =>
        {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CsrfToken_InvalidateToken);
            browser.WaitUntilDotvvmInited();

            var result = browser.Single("result", SelectByDataUi);
            var testButton = browser.Single("test-button", SelectByDataUi);

            var lazyCsrfToken = (bool)browser.GetJavaScriptExecutor().ExecuteScript("return (typeof dotvvm.state.$csrfToken === 'undefined')");

            testButton.Click();
            CheckRequests(
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken"
            );
            AssertUI.TextEquals(result, "1");
            
            var changeButton = browser.Single("change-button", SelectByDataUi);
            changeButton.Click();
            testButton.Click();
            CheckRequests(
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken",
                "POST 400 /FeatureSamples/CsrfToken/InvalidateToken",
                "GET 200 /_dotvvm/csrfToken",
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken"
            );
            AssertUI.TextEquals(result, "2");

            var eraseButton = browser.Single("erase-button", SelectByDataUi);
            eraseButton.Click();
            testButton.Click();
            CheckRequests(
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken",
                "POST 400 /FeatureSamples/CsrfToken/InvalidateToken",
                "GET 200 /_dotvvm/csrfToken",
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken",
                "GET 200 /_dotvvm/csrfToken",
                "POST 200 /FeatureSamples/CsrfToken/InvalidateToken"
            );
            AssertUI.TextEquals(result, "3");

            void CheckRequests(params string[] expected)
            {
                if (lazyCsrfToken)
                {
                    expected = new[] { "GET 200 /_dotvvm/csrfToken" }.Concat(expected).ToArray();
                }

                var items = browser.FindElements("#request-log li");
                items.ThrowIfDifferentCountThan(expected.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    AssertUI.TextEquals(items[i], expected[i]);
                }
            }
        });
    }

}
