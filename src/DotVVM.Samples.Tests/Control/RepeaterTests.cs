
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class RepeaterTests : AppSeleniumTest
    {
        [TestMethod]
        public void Control_Repeater_DataSourceNull()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_DataSourceNull);
                browser.Wait();

                var clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                var serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.AreEqual(0, clientRepeater.Children.Count);
                Assert.AreEqual(0, serverRepeater.Children.Count);

                var button = browser.Single("set-collection-button", this.SelectByDataUi);
                button.Click().Wait();

                clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.AreEqual(3, clientRepeater.Children.Count);
                Assert.AreEqual(3, serverRepeater.Children.Count);
            });
        }

        [TestMethod]
        public void Control_Repeater_NestedRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_NestedRepeater);
                browser.Wait();

                browser.ElementAt("a", 0).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 1");

                browser.ElementAt("a", 1).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 2");

                browser.ElementAt("a", 2).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 3");

                browser.ElementAt("a", 3).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 1");

                browser.ElementAt("a", 4).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 2");

                browser.ElementAt("a", 5).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 3 Subchild 1");

                browser.ElementAt("a", 6).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 1");

                browser.ElementAt("a", 7).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 2");

                browser.ElementAt("a", 8).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 1 Subchild 3");

                browser.ElementAt("a", 9).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 1");

                browser.ElementAt("a", 10).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 2 Subchild 2");

                browser.ElementAt("a", 11).Click();

                browser.First("#result").CheckIfInnerTextEquals("Child 3 Subchild 1");
            });
        }

        [TestMethod]
        public void Control_Repeater_RepeaterAsSeparator()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator);
                browser.Wait();

                var repeater = browser.Single("root-repeater", this.SelectByDataUi);
                var separators =repeater.FindElements("separator", this.SelectByDataUi);
                Assert.AreEqual(2, separators.Count);

                foreach (var separator in separators)
                {
                    var texts = separator.FindElements("p");
                    Assert.AreEqual(3, texts.Count);
                    texts[0].CheckIfTextEquals("First separator");
                    texts[1].CheckIfTextEquals("Second separator");
                    texts[2].CheckIfTextEquals("Third separator");
                }
            });
        }

        [TestMethod]
        public void Control_Repeater_RepeaterWrapperTag()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterWrapperTag);

                browser.FindElements("#part1>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1>div>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part1>div>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part1>div>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part1>div>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part1>div>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part2>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2>ul>li").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part2>ul>li", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part2>ul>li", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part2>ul>li", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part2>ul>li", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part3>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part3>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part3>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part3>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part3>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part1_server>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1_server>div>p").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part1_server>div>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part1_server>div>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part1_server>div>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part1_server>div>p", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part2_server>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2_server>ul>li").ThrowIfDifferentCountThan(4);

                browser.ElementAt("#part2_server>ul>li", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part2_server>ul>li", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part2_server>ul>li", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part2_server>ul>li", 3).CheckIfInnerTextEquals("Test 4");

                browser.FindElements("#part3_server>p").ThrowIfDifferentCountThan(4);
                browser.ElementAt("#part3_server>p", 0).CheckIfInnerTextEquals("Test 1");
                browser.ElementAt("#part3_server>p", 1).CheckIfInnerTextEquals("Test 2");
                browser.ElementAt("#part3_server>p", 2).CheckIfInnerTextEquals("Test 3");
                browser.ElementAt("#part3_server>p", 3).CheckIfInnerTextEquals("Test 4");
            });
        }

        [TestMethod]
        public void Control_Repeater_RouteLink()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLink);

                // verify link urls
                var url = browser.CurrentUrl;
                browser.ElementAt("a", 0).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 1).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 2).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 3).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 4).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 5).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 6).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 7).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 8).CheckAttribute("href", url + "/3");
                browser.ElementAt("a", 9).CheckAttribute("href", url + "/1");
                browser.ElementAt("a", 10).CheckAttribute("href", url + "/2");
                browser.ElementAt("a", 11).CheckAttribute("href", url + "/3");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [TestMethod]
        public void Control_Repeater_RouteLinkUrlSuffix()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkUrlSuffix);

                // verify link urls
                var url = browser.CurrentUrl;
                browser.ElementAt("a", 0).CheckAttribute("href", url + "/1?test");
                browser.ElementAt("a", 1).CheckAttribute("href", url + "/2?test");
                browser.ElementAt("a", 2).CheckAttribute("href", url + "/3?test");
                browser.ElementAt("a", 3).CheckAttribute("href", url + "/1?test");
                browser.ElementAt("a", 4).CheckAttribute("href", url + "/2?test");
                browser.ElementAt("a", 5).CheckAttribute("href", url + "/3?test");
                browser.ElementAt("a", 6).CheckAttribute("href", url + "/1?id=1");
                browser.ElementAt("a", 7).CheckAttribute("href", url + "/2?id=2");
                browser.ElementAt("a", 8).CheckAttribute("href", url + "/3?id=3");
                browser.ElementAt("a", 9).CheckAttribute("href", url + "/1?id=1");
                browser.ElementAt("a", 10).CheckAttribute("href", url + "/2?id=2");
                browser.ElementAt("a", 11).CheckAttribute("href", url + "/3?id=3");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [TestMethod]
        public void Control_Repeater_RouteLinkQuery()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkQuery);

                // verify link urls
                var url = browser.CurrentUrl;
                browser.ElementAt("a", 0).CheckAttribute("href", url + "?Static=query&Id=1");
                browser.ElementAt("a", 1).CheckAttribute("href", url + "?Static=query&Id=2");
                browser.ElementAt("a", 2).CheckAttribute("href", url + "?Static=query&Id=3");
                browser.ElementAt("a", 3).CheckAttribute("href", url + "?Static=query&Id=1");
                browser.ElementAt("a", 4).CheckAttribute("href", url + "?Static=query&Id=2");
                browser.ElementAt("a", 5).CheckAttribute("href", url + "?Static=query&Id=3");
                browser.ElementAt("a", 6).CheckAttribute("href", url + "?first=param&Static=query&Id=1#test");
                browser.ElementAt("a", 7).CheckAttribute("href", url + "?first=param&Static=query&Id=2#test");
                browser.ElementAt("a", 8).CheckAttribute("href", url + "?first=param&Static=query&Id=3#test");
                browser.ElementAt("a", 9).CheckAttribute("href", url + "?first=param&Static=query&Id=1#test");
                browser.ElementAt("a", 10).CheckAttribute("href", url + "?first=param&Static=query&Id=2#test");
                browser.ElementAt("a", 11).CheckAttribute("href", url + "?first=param&Static=query&Id=3#test");

                for (int i = 0; i < 12; i++)
                {
                    browser.ElementAt("a", i).CheckIfInnerText(s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [TestMethod]
        public void Control_Repeater_Separator()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_Separator);

                CheckSeparators(browser, "server-repeater");
                CheckSeparators(browser, "client-repeater");
            });
        }
        private void CheckSeparators(IBrowserWrapperFluentApi browser, string repeaterDataUi)
        {
            var repeater = browser.Single(repeaterDataUi, this.SelectByDataUi);
            for (int i = 0; i < repeater.Children.Count; i++)
            {
                if (i % 2 == 0)
                {
                    repeater.Children[i].CheckAttribute("data-ui", s => s == "item");
                }
                else
                {
                    repeater.Children[i].CheckAttribute("data-ui", s => s == "separator");
                }
            }
        }
    }
}