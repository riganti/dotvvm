using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class HierarchyRepeaterTests : AppSeleniumTest
    {
        public HierarchyRepeaterTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public void Control_HierarchyRepeater_Basic()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_HierarchyRepeater_Basic);

                AssertUI.InnerTextEquals(browser.First("HR-Empty", SelectByDataUi), "");
                AssertUI.InnerTextEquals(browser.First("HR-EmptyData", SelectByDataUi), "There are no nodes.");

                Assert.Equal(
                    new []{ "p", "p", "p", "p", "p" },
                    browser.Single("HR-NoTags", SelectByDataUi).FindElements("*").Select(e => e.GetTagName().ToLowerInvariant())
                );

                IElementWrapper getNode(string hr, params int[] index)
                {
                    var selector = string.Join(" > ", index.Select(i => $"ul > li:nth-child({i + 1})"));
                    return browser.Single($"*[data-ui={hr}] > {selector}");
                }

                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 1, 0).Single("input[type=button]"), "0");
                getNode("HR-Server", 0, 1, 0).Single("input[type=button]").Click(); // body > div:nth-child(1) > div:nth-child(3) > ul:nth-child(1) > li:nth-child(1) > ul:nth-child(3) > li:nth-child(2) > ul:nth-child(3) > li:nth-child(1) > input:nth-child(2)
                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 1, 0).Single("input[type=button]"), "1");
                AssertUI.InnerTextEquals(getNode("HR-Client", 0, 1, 0).Single("input[type=button]"), "1");

                getNode("HR-Client", 0, 0).Single("input[type=button]").Click().Click();
                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 0).Single("input[type=button]"), "2");
                AssertUI.InnerTextEquals(getNode("HR-Client", 0, 0).Single("input[type=button]"), "2");

                browser.Single("GlobalLabel", SelectByDataUi).ClearInputByKeyboard().SendKeys("lalala");
                getNode("HR-Client", 0, 0).Single("input[type=button]").Click();
                AssertUI.Attribute(getNode("HR-Server", 0, 0).Single("input[type=button]"), "title", "lalala: -- 0");
            });
        }


        [Fact]
        public void Control_HierarchyRepeater_WithMarkupControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_HierarchyRepeater_WithMarkupControl);

                IElementWrapper getNode(string hr, params int[] index)
                {
                    var selector = string.Join(" > ", index.Select(i => $"ul > li:nth-child({i + 1})"));
                    return browser.Single($"*[data-ui={hr}] > {selector} > div > div[data-ui=NodeControl]");
                }

                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 1, 0).Single("input[type=button]"), "0");
                getNode("HR-Server", 0, 1, 0).Single("input[type=button]").Click(); 
                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 1, 0).Single("input[type=button]"), "1");
                AssertUI.InnerTextEquals(getNode("HR-Client", 0, 1, 0).Single("input[type=button]"), "1");

                getNode("HR-Client", 0, 0).Single("input[type=button]").Click().Click();
                AssertUI.InnerTextEquals(getNode("HR-Server", 0, 0).Single("input[type=button]"), "2");
                AssertUI.InnerTextEquals(getNode("HR-Client", 0, 0).Single("input[type=button]"), "2");


                getNode("HR-Client", 0).Single("input[type=button]").Click().Click().Click();
                AssertUI.InnerTextEquals(getNode("HR-Server", 0).Single("input[type=button]"), "3");
                AssertUI.InnerTextEquals(getNode("HR-Client", 0).Single("input[type=button]"), "3");
                AssertUI.InnerTextEquals(browser.First("*[data-ui=Repeater] div[data-ui=NodeControl] input[type=button]"), "3");
            });
        }
    }
}
