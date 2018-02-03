using System;
using System.Linq;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class MarkupControlTests : AppSeleniumTest
    {
        public MarkupControlTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_MarkupControl_HierarchyControlPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_HierarchyControlPage);

                var prefixTextbox = browser.Single("[data-ui=prefix-text-textbox]");
                var titles = browser.FindElements("[data-ui=title]");

                void AssertTitlesContainsPrefix()
                {
                    foreach (var title in titles)
                    {
                        Assert.Contains(prefixTextbox.GetText(), title.GetInnerText());
                    }
                }

                AssertTitlesContainsPrefix();
                prefixTextbox.Clear();
                prefixTextbox.SendKeys("123");
                prefixTextbox.SendEnterKey();
                AssertTitlesContainsPrefix();

                var titleTextbox = browser.Single("[data-ui=new-title-text-textbox]").SendKeys("test");
                foreach (var button in browser.FindElements("[data-ui=static-command]"))
                {
                    button.Click();
                }

                foreach (var title in titles.Skip(1))
                {
                    var titleText =
                        "123" + string.Concat(Enumerable.Repeat(" & test", ParentElementsCount(title, "li")));
                    Assert.Equal(titleText, title.GetInnerText());
                }
            });
        }

        private int ParentElementsCount(IElementWrapper element, string tagName)
        {
            if (element.GetTagName().Equals("body", StringComparison.InvariantCultureIgnoreCase))
            {
                return 0;
            }

            return element.GetTagName() == tagName
                ? ParentElementsCount(element.ParentElement, tagName) + 1
                : ParentElementsCount(element.ParentElement, tagName);
        }
        [Fact(Skip =  "Does not work on CI. IT NEED TO BE INVESTIGATED.")]
        public void Feature_MarkupControl_ControlControlCommandInvokeAction()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlControlCommandInvokeAction);
                browser.Wait(1000);
                var allButtons = browser.First("#buttons").FindElements("button");
                foreach (var button in allButtons)
                {
                    button.Click();
                    browser.WaitFor(() => {
                        var parent = button.ParentElement.ParentElement;
                        var value = parent.First("[data-id='Row']").GetText().Trim() + "|" + parent.First("[data-id=Column]").GetText().Trim();
                        AssertUI.InnerTextEquals(browser.First("#value"),value);

                    }, 1500, "Button did not invoke action or action was not performed.");
                }


            });

        }

    }
}
