﻿using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class FormattingTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Formatting_Formatting()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Formatting_Formatting);

                // verify items rendered on client and on the server are the same
                var items1 = browser.FindElements(".list1 li");
                var items2 = browser.FindElements(".list2 li");
                AssertUI.InnerTextEquals(items1.ElementAt(0), items2.ElementAt(0).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(1), items2.ElementAt(1).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(2), items2.ElementAt(2).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(3), items2.ElementAt(3).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(4), items2.ElementAt(4).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(5), items2.ElementAt(5).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(6), items2.ElementAt(6).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(7), items2.ElementAt(7).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(8), items2.ElementAt(8).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(9), items2.ElementAt(9).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(10), items2.ElementAt(10).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(11), items2.ElementAt(11).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(12), items2.ElementAt(12).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(13), items2.ElementAt(13).GetText());

                // do the postback
                browser.Click("input[type=button]");

                // verify items rendered on client and on the server are the same
                items1 = browser.FindElements(".list1 li");
                items2 = browser.FindElements(".list2 li");
                AssertUI.InnerTextEquals(items1.ElementAt(0), items2.ElementAt(0).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(1), items2.ElementAt(1).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(2), items2.ElementAt(2).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(3), items2.ElementAt(3).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(4), items2.ElementAt(4).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(5), items2.ElementAt(5).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(6), items2.ElementAt(6).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(7), items2.ElementAt(7).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(8), items2.ElementAt(8).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(9), items2.ElementAt(9).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(10), items2.ElementAt(10).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(11), items2.ElementAt(11).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(12), items2.ElementAt(12).GetText());
                AssertUI.InnerTextEquals(items1.ElementAt(13), items2.ElementAt(13).GetText());
            });
        }

        [Fact]
        public void Feature_Formatting_ToStringGlobalFunctionBug()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Formatting_ToStringGlobalFunctionBug);

                AssertUI.TextEquals(browser.Single(".result-boolean"), "Boolean: false");
                AssertUI.TextEquals(browser.Single(".result-string"), "String:");
                browser.Single("input[type=button]").Click();
                AssertUI.TextEquals(browser.Single(".result-boolean"), "Boolean: true");
                AssertUI.TextEquals(browser.Single(".result-string"), "String:");
            });
        }

        [Fact]
        public void Feature_Formatting_AutoResourceInclusion()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Formatting_AutoResourceInclusion);

                browser.FindElements("p").ThrowIfDifferentCountThan(3);
            });
        }

        public FormattingTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
