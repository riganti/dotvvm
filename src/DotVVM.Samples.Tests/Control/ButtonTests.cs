using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class ButtonTests : AppSeleniumTest
    {
        public ButtonTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void Control_Button_Button()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_Button);
                var resultCheck = browser.First("span.result");
                AssertUI.InnerText(resultCheck, s => s.Equals("0"), "Text has to be '0'");

                browser.First("input[type=button]").Click();
                AssertUI.InnerText(resultCheck, s => s.Equals("1"), "Text has to be '1'");
            });
        }
        [Fact]
        public void Control_Button_InputTypeButton_TextContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_TextContentInside);

                AssertUI.Attribute(browser.First("input[type=button]"),"value", s => s.Equals("This is text"));
            });
        }

        [Fact]
        public void Control_Button_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);

                AssertUI.InnerText(browser.First("p.summary"),
                      t =>
                          t.Trim().Contains("DotVVM.Framework.Controls.DotvvmControlException") &&
                          t.Trim().Contains("The <dot:Button> control cannot have inner HTML connect unless the 'ButtonTagName' property is set to 'button'!")
                      , "");
            });
        }

        [Fact]
        public void Control_Button_ButtonTagName()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonTagName);

                AssertUI.TagName(browser.First("#ButtonTextProperty"), s => s.Equals("button"));
                AssertUI.TagName(browser.First("#ButtonTextBinding"), s => s.Equals("button"));
                AssertUI.TagName(browser.First("#InputTextProperty"), s => s.Equals("input"));
                AssertUI.TagName(browser.First("#InputTextBinding"), s => s.Equals("input"));
                AssertUI.TagName(browser.First("#ButtonInnerText"), s => s.Equals("button"));

                AssertUI.TagName(browser.First("#ButtonTextPropertyUpperCase"), s => s.Equals("button"));
                AssertUI.TagName(browser.First("#ButtonTextBindingUpperCase"), s => s.Equals("button"));
                AssertUI.TagName(browser.First("#InputTextPropertyUpperCase"), s => s.Equals("input"));
                AssertUI.TagName(browser.First("#ButtonInnerTextUpperCase"), s => s.Equals("button"));
            });
        }

        [Fact]
        public void Control_Button_ButtonOnclick()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonOnclick);

                var onclickResult = browser.First("span.result1");
                var clickResult = browser.First("span.result2");

                AssertUI.InnerText(clickResult, s => s.Equals(""));
                AssertUI.InnerText(onclickResult, s => s.Equals(""));

                browser.First("input[type=button]").Click();
                AssertUI.InnerText(clickResult, s => s.Equals("Changed from command binding"));
                AssertUI.InnerText(onclickResult, s => s.Contains("Changed from onclick"));
            });
        }

        [Fact]
        public void Control_Button_ButtonEnabled()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonEnabled);

                var commandResult = browser.First("[data-ui=command-result]");
                var staticCommandResult = browser.First("[data-ui=static-command-result]");
                var clientStaticCommandResult = browser.First("[data-ui=client-static-command-result]");

                AssertUI.InnerTextEquals(commandResult, "");
                AssertUI.InnerTextEquals(staticCommandResult, "");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "");

                var commandButton = browser.First("[data-ui=command-button]");
                var staticCommandButton = browser.First("[data-ui=static-command-button]");
                var clientStaticCommandButton = browser.First("[data-ui=client-static-command-button]");

                commandButton.Click();
                staticCommandButton.Click();
                clientStaticCommandButton.Click();

                AssertUI.InnerTextEquals(commandResult, "");
                AssertUI.InnerTextEquals(staticCommandResult, "");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "");

                browser.First("[data-ui=toggle-enabled]").Click();

                commandButton.Click();
                staticCommandButton.Click();
                clientStaticCommandButton.Click();

                AssertUI.InnerTextEquals(commandResult, "Changed from command binding");
                AssertUI.InnerTextEquals(staticCommandResult, "Changed from static command on server");
                AssertUI.InnerTextEquals(clientStaticCommandResult, "Changed from static command");
            });
        }
    }
}
