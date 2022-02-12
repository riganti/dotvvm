using System;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Api;
using Riganti.Selenium.DotVVM;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
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

        [Fact]
        public void Feature_MarkupControl_ControlControlCommandInvokeAction()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlControlCommandInvokeAction);
                browser.WaitUntilDotvvmInited();

                var allButtons = browser.First("#buttons").FindElements("button");
                foreach (var button in allButtons)
                {
                    button.Click();
                    browser.WaitFor(() => {
                        var parent = button.ParentElement.ParentElement;
                        var value = parent.First("[data-id='Column2']").GetText().Trim() + "|" + parent.First("[data-id=Row2]").GetText().Trim() + "|" + parent.First("[data-id='Row']").GetText().Trim() + "|" + parent.First("[data-id=Column]").GetText().Trim();

                        AssertUI.InnerTextEquals(browser.First("#value"), value);
                    },
                    8000, // sometimes chrome takes more time to negotiate with proxy (avg 3s) 
                    "Button did not invoke action or action was not performed.");
                }

                AssertUI.TextEquals(browser.First("#Duplicity"), "false");
            });
        }

        [Fact]
        public void Feature_MarkupControl_CommandBindingInRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_CommandBindingInRepeater);
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Hello from DotVVM!");

                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action1 - Item 1"); ;

                browser.ElementAt("input[type=button]", 1).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action2 - Item 1");

                browser.ElementAt("input[type=button]", 2).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action1 - Item 2");

                browser.ElementAt("input[type=button]", 3).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action2 - Item 2");

                browser.ElementAt("input[type=button]", 4).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action1 - Item 3");

                browser.ElementAt("input[type=button]", 5).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "Action2 - Item 3");
            });
        }

        [Fact]
        public void Feature_MarkupControl_CommandBindingInDataContextWithControlProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_CommandBindingInDataContextWithControlProperty);

                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result1]"), "Init");
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result2]"), "Init");

                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result1]"), "changed");
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result2]"), "Init");

                browser.ElementAt("input[type=button]", 1).Click();
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result1]"), "changed");
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result2]"), "changed");
            });
        }

        [Fact]
        public void Feature_MarkupControl_ControlPropertyUpdatedByServer()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyUpdatedByServer);

                AssertUI.Value(browser.ElementAt("input[data-uitest=editor]", 0), "false");
                browser.First("input[data-uitest=simpleProperty]").SendKeys("test");
                browser.First("input[data-uitest=simpleProperty]").SendKeys(Keys.Tab);
                AssertUI.Value(browser.ElementAt("input[data-uitest=editor]", 0), "true");
                browser.First("input[data-uitest=simpleProperty]").Clear().SendKeys("test2");
                browser.First("input[data-uitest=simpleProperty]").SendKeys(Keys.Tab);
                AssertUI.Value(browser.ElementAt("input[data-uitest=editor]", 0), "false");

                AssertUI.Value(browser.ElementAt("input[data-uitest=editor]", 1), "");
                AssertUI.Value(browser.First("input[data-uitest=childProperty]"), "");
                browser.First("input[data-uitest=childPropertyButton]").Click();
                AssertUI.Value(browser.ElementAt("input[data-uitest=editor]", 1), "TEST");
                AssertUI.Value(browser.First("input[data-uitest=childProperty]"), "TEST");
            });
        }

        [Fact]
        public void Feature_MarkupControl_ControlPropertyUpdating()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyUpdating);

                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "TEST 123");
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "TEST 123 HUHA");
                browser.First("input[type=button]").Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "ABC FFF");
                AssertUI.InnerTextEquals(browser.First("span[data-uitest=result]"), "ABC FFF HUHA");
            });
        }

        [Fact]
        public void Feature_MarkupControl_ControlPropertyValidationPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyValidationPage);

                browser.Single("input[type=button]").Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.InnerTextEquals(browser.First("li"), "The Text field is required.");
                AssertUI.InnerTextEquals(browser.Single("span"), "VALIDATION ERROR");

                browser.ElementAt("input[type=text]", 0).SendKeys("test");
                browser.Single("input[type=button]").Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "test");
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.InnerTextEquals(browser.First("li"), "The Text field is not a valid e-mail address.");
                AssertUI.InnerTextEquals(browser.Single("span"), "VALIDATION ERROR");

                browser.ElementAt("input[type=text]", 0).SendKeys("@mail.com");
                browser.Single("input[type=button]").Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "test@mail.com");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                AssertUI.InnerTextEquals(browser.Single("span"), "");

                browser.ElementAt("input[type=text]", 0).Clear();
                browser.Single("input[type=button]").Click();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                AssertUI.InnerTextEquals(browser.First("li"), "The Text field is required.");
                AssertUI.InnerTextEquals(browser.Single("span"), "VALIDATION ERROR");
            });
        }

        [Fact]
        public void Feature_MarkupControl_MarkupControlRegistration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_MarkupControlRegistration);

                AssertUI.InnerTextEquals(browser.ElementAt("h2", 0), "First Control");
                AssertUI.InnerTextEquals(browser.ElementAt("h2", 1), "Second control name was set from the binding");

                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "15");
                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "16");

                browser.ElementAt("input[type=button]", 0).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "17");

                browser.ElementAt("input[type=button]", 1).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 0), "16");

                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "25");
                browser.ElementAt("input[type=button]", 2).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "26");

                browser.ElementAt("input[type=button]", 2).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "27");

                browser.ElementAt("input[type=button]", 3).Click();
                AssertUI.Value(browser.ElementAt("input[type=text]", 1), "26");
            });
        }

        [Fact]
        public void Feature_MarkupControl_MultiControlHierarchy()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_MultiControlHierarchy);
                WaitForExecutor.WaitFor(() => {
                    var ul = browser.First("ul", By.CssSelector);
                    ul.FindElements("li", By.CssSelector).ThrowIfDifferentCountThan(20, WaitForOptions.Disabled);
                });
            });
        }

        [Fact]
        public void Feature_MarkupControl_ResourceBindingInControlProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ResourceBindingInControlProperty);

                var label = browser.First("[data-ui=markup-control] label");
                var input = browser.First("[data-ui=markup-control] input");

                AssertUI.InnerTextEquals(label, "Sample Text");
                AssertUI.InnerTextEquals(input, "Sample Text");

                input.SendKeys("123");
                input.SendEnterKey();

                AssertUI.InnerTextEquals(label, "Sample Text");
            });
        }

        [Fact]
        public void Feature_MarkupControl_ComboBoxDataSourceBoundToStaticCollection()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ComboBoxDataSourceBoundToStaticCollection);

                var innerControlLiteral = browser.First("[data-ui=inner-control-literal]");
                AssertUI.InnerTextEquals(innerControlLiteral, "Default item");

                var comboboxSelectedValue = browser.First("[data-ui=combobox-selected-value]");
                var combobox = browser.First("[data-ui=combobox]");

                combobox.Select(0);
                AssertUI.InnerTextEquals(comboboxSelectedValue, "0");

                combobox.Select(1);
                AssertUI.InnerTextEquals(comboboxSelectedValue, "1");

                combobox.Select(2);
                AssertUI.InnerTextEquals(comboboxSelectedValue, "2");

                AssertUI.InnerTextEquals(combobox.ElementAt("option", 0), "Number 0");
                AssertUI.InnerTextEquals(combobox.ElementAt("option", 1), "Number 1");
                AssertUI.InnerTextEquals(combobox.ElementAt("option", 2), "Number 2");
            });
        }

        [Fact]
        public void Feature_MarkupControl_CommandPropertiesInMarkupControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_CommandPropertiesInMarkupControl);

                var ok = browser.First("[data-ui=ok]");
                var body = browser.First("body");
                var span = browser.First("[data-ui=result]");

                AssertUI.NotContainsElement(body, "[data-ui=cancel]");
                ok.Click();

                AssertUI.InnerTextEquals(span, "Command result.");
            });
        }

        [Fact]
        public void Feature_MarkupControl_StaticCommandInMarkupControl()
        {
            RunInAllBrowsers(browser => {
                // clean the state 
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_StaticCommandInMarkupControl);
                browser.WaitUntilDotvvmInited();

                browser.First("[data-ui=reset]").Click();
                AssertUI.TextEquals(browser.First("[data-ui='test-state']"), "OK", failureMessage: "Test could not clear state.");

                // start the test over
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_StaticCommandInMarkupControl);
                browser.WaitUntilDotvvmInited();

                // button selectors
                Func<IElementWrapper> save = () => browser.First("[data-ui=save]");
                Func<IElementWrapper> input = () => browser.First("[data-ui=input]");
                Func<IElementWrapper> cancel = () => browser.First("[data-ui=blank]");


                Func<IElementWrapper> editButton = () => browser.First("article").First("[data-uitest-name='edit']");
                Func<IElementWrapper> removeButton = () => browser.First("article").First("[data-uitest-name='remove']");

                input().Clear().SendKeys("test1");
                save().Click();

                browser.WaitFor(() => AssertUI.TextEquals(browser.Last("article>span"), "test1"), 2000);


                editButton().Click();
                input().Clear().SendKeys("changed");
                save().Click();

                AssertUI.Any(browser.FindElements("article>span")).TextEquals("changed");

                editButton().Click();
                input().Clear().SendKeys("changed2");
                save().Click();

                browser.WaitFor(() => AssertUI.Any(browser.FindElements("article>span")).TextEquals("changed2"), 2000);
                browser.WaitFor(() => AssertUI.All(browser.FindElements("article>span")).TextNotEquals("changed"), 2000);

                removeButton().Click();
                browser.WaitFor(() => AssertUI.All(browser.FindElements("article>span")).TextNotEquals("changed2"), 2000);
                browser.WaitFor(() => AssertUI.All(browser.FindElements("article>span")).TextNotEquals("changed"), 2000);
            });
        }

        [Fact]
        public void Feature_MarkupControl_MarkupDeclaredProperties()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_MarkupDefinedProperties);
                browser.WaitUntilDotvvmInited();

                Func<int, IElementWrapper> listElement = (int index) => browser.Single($"li[data-ui='list-item']:nth-child({index + 1})");
                Func<int> listLenght = () => browser.FindElements("li[data-ui='list-item']").Count;

                Func<int, int, IElementWrapper> arrayElement = (int x, int y) =>
                    browser.Single($"li[data-ui='2d-array-x']:nth-child({x + 1}) li[data-ui='2d-array-y']:nth-child({y + 1})");

                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(arrayElement(0, 0), "a");
                    AssertUI.InnerTextEquals(arrayElement(1, 0), "b");
                    AssertUI.InnerTextEquals(arrayElement(1, 1), "c");
                    AssertUI.InnerTextEquals(arrayElement(2, 0), "d");
                }, 2000);

                browser.WaitFor(() => {
                    Assert.Equal(4, listLenght());

                    AssertUI.InnerTextEquals(listElement(0), "Barn");
                    AssertUI.InnerTextEquals(listElement(1), "House");
                    AssertUI.InnerTextEquals(listElement(2), "Skyscraper");
                    AssertUI.InnerTextEquals(listElement(3), "Town Hall");
                }, 2000);

                browser.Single("input[data-ui='item-name']").SendKeys("Cabin");
                browser.Single("input[data-ui='add-item']").Click();

                browser.WaitFor(() => Assert.Equal(5, listLenght()), 2000);


                browser.WaitFor(() => AssertUI.InnerTextEquals(listElement(0), "Barn"), 2000);
                browser.WaitFor(() => AssertUI.InnerTextEquals(listElement(1), "House"), 2000);
                browser.WaitFor(() => AssertUI.InnerTextEquals(listElement(2), "Skyscraper"), 2000);
                browser.WaitFor(() => AssertUI.InnerTextEquals(listElement(3), "Town Hall"), 2000);
                browser.WaitFor(() => AssertUI.InnerTextEquals(listElement(4), "Cabin"), 2000);

                browser.Single("input[data-ui='increase-counter']").Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(browser.First("[data-ui=counter]"), "1"), 2000);

                browser.Single("input[data-ui='increase-counter']").Click();
                browser.WaitFor(() => AssertUI.InnerTextEquals(browser.First("[data-ui=counter]"), "2"), 2000);
            });
        }
    }
}
