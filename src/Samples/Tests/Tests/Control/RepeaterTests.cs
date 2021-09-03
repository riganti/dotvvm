using System;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class RepeaterTests : AppSeleniumTest
    {
        public RepeaterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_Repeater_DataSourceNull()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_DataSourceNull);

                var clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                var serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(0, clientRepeater.Children.Count);
                Assert.Equal(0, serverRepeater.Children.Count);

                var button = browser.Single("set-collection-button", this.SelectByDataUi);
                button.Click();

                clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(3, clientRepeater.Children.Count);
                Assert.Equal(3, serverRepeater.Children.Count);
            });
        }

        [Fact]
        public void Control_Repeater_RepeaterAsSeparator()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator);

                var repeater = browser.Single("root-repeater", this.SelectByDataUi);

                for (int i = 2; i < 5; i++)
                {
                    var separators = repeater.FindElements("separator", this.SelectByDataUi);

                    Assert.Equal(i, separators.Count);

                    foreach (var separator in separators)
                    {
                        var texts = separator.FindElements("p");
                        Assert.Equal(3, texts.Count);
                        AssertUI.InnerTextEquals(texts[0], "First separator");
                        AssertUI.InnerTextEquals(texts[1], "Second separator");
                        AssertUI.InnerTextEquals(texts[2], "Third separator");
                    }

                    browser.Single("add-item-button", SelectByDataUi).Click();
                    browser.WaitForPostback();
                }
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator))]
        public void Control_Repeater_RepeaterAsSeparator_CorrectBindingContext()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator);

                var repeater = browser.Single("root-repeater", this.SelectByDataUi);

                var incrementButtons = repeater.FindElements("increment-button", this.SelectByDataUi);

                var counterValue = browser.Single("counter-value", this.SelectByDataUi);
                AssertUI.InnerTextEquals(counterValue, "0");

                var counter = 1;
                foreach (var button in incrementButtons)
                {
                    button.Click();
                    AssertUI.InnerTextEquals(counterValue, counter.ToString(), failureMessage: "Counter value invalid!");
                    counter++;
                }

                browser.Single("add-item-button", SelectByDataUi).Click();
                incrementButtons = repeater.FindElements("increment-button", this.SelectByDataUi);

                foreach (var button in incrementButtons)
                {
                    button.Click();
                AssertUI.InnerTextEquals(counterValue, counter.ToString(),failureMessage:"Counter value invalid!");
                    counter++;
                }
            });
        }

        [Fact]
        public void Control_Repeater_RepeaterWrapperTag()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterWrapperTag);

                browser.FindElements("#part1>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1>div>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 3), "Test 4");

                browser.FindElements("#part2>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2>ul>li").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 3), "Test 4");

                browser.FindElements("#part3>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 3), "Test 4");

                browser.FindElements("#part1_server>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1_server>div>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 3), "Test 4");

                browser.FindElements("#part2_server>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2_server>ul>li").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 3), "Test 4");

                browser.FindElements("#part3_server>p").ThrowIfDifferentCountThan(4);
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 3), "Test 4");
            });
        }

        [Fact]
        public void Control_Repeater_RouteLink()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLink);

                // verify link urls
                var url = browser.CurrentUrl;
                AssertUI.Attribute(browser.ElementAt("a", 0), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 1), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 2), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 3), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 4), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 5), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 6), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 7), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 8), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 9), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 10), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 11), "href", url + "/3");

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_RouteLinkUrlSuffix()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkUrlSuffix);

                // verify link urls
                var url = browser.CurrentUrl;
                AssertUI.Attribute(browser.ElementAt("a", 0), "href", url + "/1?test");
                AssertUI.Attribute(browser.ElementAt("a", 1), "href", url + "/2?test");
                AssertUI.Attribute(browser.ElementAt("a", 2), "href", url + "/3?test");
                AssertUI.Attribute(browser.ElementAt("a", 3), "href", url + "/1?test");
                AssertUI.Attribute(browser.ElementAt("a", 4), "href", url + "/2?test");
                AssertUI.Attribute(browser.ElementAt("a", 5), "href", url + "/3?test");
                AssertUI.Attribute(browser.ElementAt("a", 6), "href", url + "/1?id=1");
                AssertUI.Attribute(browser.ElementAt("a", 7), "href", url + "/2?id=2");
                AssertUI.Attribute(browser.ElementAt("a", 8), "href", url + "/3?id=3");
                AssertUI.Attribute(browser.ElementAt("a", 9), "href", url + "/1?id=1");
                AssertUI.Attribute(browser.ElementAt("a", 10), "href", url + "/2?id=2");
                AssertUI.Attribute(browser.ElementAt("a", 11), "href", url + "/3?id=3");

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_RouteLinkQuery()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkQuery);

                // verify link urls
                var url = browser.CurrentUrl;

                AssertUI.HyperLinkEquals(browser.ElementAt("a", 0), url + "?Static=query&Id=1", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 1), url + "?Static=query&Id=2", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 2), url + "?Static=query&Id=3", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 3), url + "?Static=query&Id=1", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 4), url + "?Static=query&Id=2", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 5), url + "?Static=query&Id=3", UrlKind.Absolute, false, UriComponents.PathAndQuery);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 6), url + "?first=param&Static=query&Id=1#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 7), url + "?first=param&Static=query&Id=2#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 8), url + "?first=param&Static=query&Id=3#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 9), url + "?first=param&Static=query&Id=1#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 10), url + "?first=param&Static=query&Id=2#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);
                AssertUI.HyperLinkEquals(browser.ElementAt("a", 11), url + "?first=param&Static=query&Id=3#test", UrlKind.Absolute, false, UriComponents.PathAndQuery | UriComponents.Fragment);

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_Separator()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_Separator);

                CheckSeparators(browser, "server-repeater");
                CheckSeparators(browser, "client-repeater");
            });
        }

        [Fact]
        public void Control_Repeater_RequiredResource()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RequiredResource);

                var clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                var serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(0, clientRepeater.Children.Count);
                Assert.Equal(0, serverRepeater.Children.Count);
            });
        }

        [Fact]
        public void Control_Repeater_CollectionIndex()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_CollectionIndex);

                var clientRenderedItems = browser.FindElements("client-rendered-item", this.SelectByDataUi);
                var serverRenderedItems = browser.FindElements("server-rendered-item", this.SelectByDataUi);
                var counterElement = browser.Single("counter", this.SelectByDataUi);

                var allItems = clientRenderedItems.Select((element, index) => (element, index))
                    .Concat(serverRenderedItems.Select((element, index) => (element, index)));

                var counter = 0;
                void CheckCounter()
                {
                    AssertUI.InnerTextEquals(counterElement, counter.ToString());
                }

                foreach (var item in allItems)
                {
                    CheckCounter();

                    foreach (var button in item.element.FindElements("input"))
                    {
                        button.Click();
                        counter += item.index;
                        CheckCounter();
                    }

                    AssertUI.InnerTextEquals(item.element.First("span"), item.index.ToString());
                }
            });
        }

        private void CheckSeparators(IBrowserWrapper browser, string repeaterDataUi)
        {
            var repeater = browser.Single(repeaterDataUi, this.SelectByDataUi);
            for (int i = 0; i < repeater.Children.Count; i++)
            {
                if (i % 2 == 0)
                {
                    AssertUI.Attribute(repeater.Children[i], "data-ui", s => s == "item");
                }
                else
                {
                    AssertUI.Attribute(repeater.Children[i], "data-ui", s => s == "separator");
                }
            }
        }
    }
}
