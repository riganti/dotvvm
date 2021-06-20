using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class StaticCommandStringMethodTests : AppSeleniumTest
    {
        public StaticCommandStringMethodTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_Contains()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='Contains(value)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click();
                textbox.SendKeys("Because");
                AssertUI.InnerTextEquals(textbox, "Because");

                button.Click();
                AssertUI.InnerTextEquals(result, "true");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("batman");
                AssertUI.InnerTextEquals(textbox, "batman");

                button.Click();
                AssertUI.InnerTextEquals(result, "false");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_EndsWith()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='EndsWith(value)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click();
                textbox.SendKeys("C#.");
                AssertUI.InnerTextEquals(textbox, "C#.");

                button.Click();
                AssertUI.InnerTextEquals(result, "true");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("C++.");
                AssertUI.InnerTextEquals(textbox, "C++.");

                button.Click();
                AssertUI.InnerTextEquals(result, "false");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_IndexOf()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='IndexOf(value)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click();
                textbox.SendKeys("glasses");
                AssertUI.InnerTextEquals(textbox, "glasses");

                button.Click();
                AssertUI.InnerTextEquals(result, "37");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("o");
                AssertUI.InnerTextEquals(textbox, "o");

                button.Click();
                AssertUI.InnerTextEquals(result, "5");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_IndexOf_StartIndex()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='IndexOf(value,30)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click();
                textbox.SendKeys("glasses");
                AssertUI.InnerTextEquals(textbox, "glasses");

                button.Click();
                AssertUI.InnerTextEquals(result, "37");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("o");
                AssertUI.InnerTextEquals(textbox, "o");

                button.Click();
                AssertUI.InnerTextEquals(result, "30");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_IsNullOrEmpty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='IsNullOrEmpty(value)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click();
                textbox.SendKeys("glasses");
                AssertUI.InnerTextEquals(textbox, "glasses");

                button.Click();
                AssertUI.InnerTextEquals(result, "false");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");

                button.Click();
                AssertUI.InnerTextEquals(result, "true");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_LastIndexOf()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='LastIndexOf(value)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click();
                textbox.SendKeys("glasses");
                AssertUI.InnerTextEquals(textbox, "glasses");

                button.Click();
                AssertUI.InnerTextEquals(result, "37");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("o");
                AssertUI.InnerTextEquals(textbox, "o");

                button.Click();
                AssertUI.InnerTextEquals(result, "63");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_LastIndexOf_StartIndex()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='LastIndexOf(value, 30)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click();
                textbox.SendKeys("glasses");
                AssertUI.InnerTextEquals(textbox, "glasses");

                button.Click();
                AssertUI.InnerTextEquals(result, "-1");

                textbox.Click();
                textbox.Clear();
                AssertUI.InnerTextEquals(textbox, "");
                textbox.SendKeys("o");
                AssertUI.InnerTextEquals(textbox, "o");

                button.Click();
                AssertUI.InnerTextEquals(result, "30");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_Replace()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                var button = browser.First($"//input[@value='Replace(a, A)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                button.Click();
                AssertUI.InnerTextEquals(result, "Why do JAvA progrAmmers hAve to weAr glAsses? BecAuse they do not C#.");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_ChangeLetters()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslation);

                var button = browser.First($"//input[@value='ToLower()']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                button.Click();
                AssertUI.InnerTextEquals(result, "why do java programmers have to wear glasses? because they do not c#.");

                button = browser.First($"//input[@value='ToUpper()']", By.XPath);
                button.Click();
                AssertUI.InnerTextEquals(result, "WHY DO JAVA PROGRAMMERS HAVE TO WEAR GLASSES? BECAUSE THEY DO NOT C#.");

            });
        }

    }
}
