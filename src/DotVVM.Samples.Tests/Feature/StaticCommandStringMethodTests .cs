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
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_Contains_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='Contains(value,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='Contains(value,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click().SendKeys("BECAUSE");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "false");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "true");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_EndsWith()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_EndsWith_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='EndsWith(value,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='EndsWith(value,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click().SendKeys("c#.");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "false");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "true");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_IndexOf()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_IndexOf_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='IndexOf(value,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='IndexOf(value,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click().SendKeys("GLASSES");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "-1");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "37");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_IndexOf_StartIndex()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_IndexOf_StartIndex_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='IndexOf(value,30,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='IndexOf(value,30,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click().SendKeys("GLASSES");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "-1");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "37");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_IsNullOrEmpty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_Join_List()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);


                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='Join(., list)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                button.Click();
                AssertUI.InnerTextEquals(result, "Real.programmers.count.from.0");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_Join_Array()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);


                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First("//input[@value='Join( JOIN , array)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                button.Click();
                AssertUI.InnerTextEquals(result, "Real JOIN programmers JOIN count JOIN from JOIN 0");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_LastIndexOf()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
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
        public void Feature_StaticCommand_String_Method_LastIndexOf_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='LastIndexOf(value,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='LastIndexOf(value,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click().SendKeys("GLASSES");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "-1");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "37");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_LastIndexOf_StartIndex()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                
                var textbox = browser.First("[data-ui=textbox]");
                var button = browser.First($"//input[@value='LastIndexOf(value,30)']", By.XPath);
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
        public void Feature_StaticCommand_String_Method_LastIndexOf_StartIndex_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='LastIndexOf(value,30,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='LastIndexOf(value,30,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=index-result]");

                textbox.Click().SendKeys("GLASSES");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "-1");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "37");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_Replace()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var button = browser.First($"//input[@value='Replace(a, A)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                button.Click();
                AssertUI.InnerTextEquals(result, "Why do JAvA progrAmmers hAve to weAr glAsses? BecAuse they do not C#.");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_Split()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var button = browser.First($"//input[@value='Split((char)?)']", By.XPath);
                var result = browser.First("[data-ui=repeater]").FindElements("p", By.TagName);


                button.Click();
                AssertUI.InnerTextEquals(result[0], "Why do Java programmers have to wear glasses");
                AssertUI.InnerTextEquals(result[1], "Because they do not C#.");
            });
        }
        [Fact]
        public void Feature_StaticCommand_String_Method_Split_ByString()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var button = browser.First($"//input[@value='Split((string)do)']", By.XPath);
                var result = browser.First("[data-ui=repeater]").FindElements("p", By.TagName);


                button.Click();
                AssertUI.InnerTextEquals(result[0], "Why");
                AssertUI.InnerTextEquals(result[1], "Java programmers have to wear glasses? Because they");
                AssertUI.InnerTextEquals(result[2], "not C#.");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_StartsWith_WithCaseSensitivity()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

                var textbox = browser.First("[data-ui=textbox]");
                var buttonCaseSensitive = browser.First($"//input[@value='StartsWith(value,InvariantCulture)']", By.XPath);
                var buttonCaseInsensitive = browser.First($"//input[@value='StartsWith(value,InvariantCultureIgnoreCase)']", By.XPath);
                var result = browser.First("[data-ui=operation-result]");

                textbox.Click().SendKeys("WHY");
                buttonCaseSensitive.Click();
                AssertUI.InnerTextEquals(result, "false");

                buttonCaseInsensitive.Click();
                AssertUI.InnerTextEquals(result, "true");
            });
        }

        [Fact]
        public void Feature_StaticCommand_String_Method_ChangeLetters()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_JavascriptTranslation_StringMethodTranslations);

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
