using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class CommandArgumentsTests : AppSeleniumTest
    {
        public CommandArgumentsTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_CommandArguments_CommandArguments()
        {
            const string Value = "testing value";

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CommandArguments_CommandArguments);

                var text = browser.Single("[data-ui='value']");
                AssertUI.InnerTextEquals(text, "Nothing here");
                browser.Single("[data-ui='button1'] button").Click();

                OpenQA.Selenium.IAlert alert = null;
                browser.WaitFor(() => {
                    alert = browser.GetAlert();
                }, 2000);

                alert.SendKeys(Value);
                alert.Accept();

                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(text, Value);
                }, 2000);
            });
        }

        [Fact]
        public void Feature_CommandArguments_CommandArgumentTypes()
        {
            const string Value = "testing value";

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CommandArguments_CommandArgumentTypes);

                var text = browser.Single("[data-ui='value']");
                AssertUI.InnerTextEquals(text, "Nothing here");

                browser.Single("[data-ui='button2'] input[type=text]").Clear().SendKeys(Value);
                browser.Single("[data-ui='button2'] button").Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(text, Value + "(from second button)");
                }, 2000);
            });
        }

        [Fact]
        public void Feature_CommandArguments_ReturnValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CommandArguments_ReturnValue);

                foreach (var button in browser.FindElements("button[data-ui]"))
                {
                    AssertUI.InnerTextEquals(button, "Uninitialized");
                }

                var refreshTextCommand1 = browser.Single("[data-ui=refresh-text-command1]");
                var refreshTextCommand2 = browser.Single("[data-ui=refresh-text-command2]");

                refreshTextCommand1.Click();
                AssertUI.InnerTextEquals(refreshTextCommand1, "Text: 0");

                refreshTextCommand2.Click();
                AssertUI.InnerTextEquals(refreshTextCommand2, "Text: 1");

                var refreshTextStaticCommand = browser.Single("[data-ui=refresh-text-static-command]");
                var getTextStaticCommand = browser.Single("[data-ui=get-text-static-command]");

                refreshTextStaticCommand.Click();
                AssertUI.InnerTextEquals(refreshTextStaticCommand, "Text: 3");

                getTextStaticCommand.Click();
                AssertUI.InnerTextEquals(getTextStaticCommand, "Text: 3");
            });
        }
    }
}
