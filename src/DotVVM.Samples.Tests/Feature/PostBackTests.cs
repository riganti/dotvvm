using System.Threading;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class PostBackTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_PostBack_PostbackUpdate()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdate);

                // enter number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(14);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(4);
            });
        }

        [Fact]
        public void Feature_PostBack_PostbackUpdateRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdateRepeater);

                // enter the text and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test");
                browser.Click("input[type=button]");

                // check the inner text of generated items
                browser.WaitFor(() => {
                    browser.FindElements(".render-server p.item")
                        .ThrowIfDifferentCountThan(5).ForEach(e => {
                            AssertUI.InnerTextEquals(e, "test");
                        });
                    browser.FindElements(".render-client p.item")
                        .ThrowIfDifferentCountThan(5).ForEach(e => {
                            AssertUI.InnerTextEquals(e, "test");
                        });
                }, 5000);

                // change the text and client the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "xxx");
                browser.Click("input[type=button]");

                // check the inner text of generated items
                browser.WaitFor(() => {
                    browser.FindElements(".render-server p.item")
                        .ThrowIfDifferentCountThan(5).ForEach(e => {
                            AssertUI.InnerTextEquals(e, "xxx");
                        });
                    browser.FindElements(".render-client p.item")
                        .ThrowIfDifferentCountThan(5).ForEach(e => {
                            AssertUI.InnerTextEquals(e, "xxx");
                        });
                }, 5000);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_PostBack_ConfirmPostBackHandler))]
        public void Feature_PostBack_PostBackHandlers_Localization()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers_Localized);
                ValidatePostbackHandlersComplexSection(".commandBinding", browser);

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers_Localized);
                ValidatePostbackHandlersComplexSection(".staticCommandBinding", browser);
            });
        }

        private void ValidatePostbackHandlersComplexSection(string sectionSelector, IBrowserWrapper browser)
        {
            IElementWrapper section = null;
            browser.WaitFor(() => {
                section = browser.First(sectionSelector);
            }, 2000, "Cannot find static commands section.");

            var index = browser.First("[data-ui=\"command-index\"]");

            // confirm first
            section.ElementAt("input[type=button]", 0).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "1");

            // cancel second
            section.ElementAt("input[type=button]", 1).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();

            AssertUI.AlertTextEquals(browser, "Confirmation 2");
            browser.DismissAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "1");
            // confirm second
            section.ElementAt("input[type=button]", 1).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.AlertTextEquals(browser, "Confirmation 2");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "2");

            // confirm third
            section.ElementAt("input[type=button]", 2).Click();
            Assert.False(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "3");

            // confirm fourth
            section.ElementAt("input[type=button]", 3).Click();
            AssertUI.AlertTextEquals(browser, "Generated 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "4");

            // confirm fifth
            section.ElementAt("input[type=button]", 4).Click();
            AssertUI.AlertTextEquals(browser, "Generated 2");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "5");

            // confirm conditional
            section.ElementAt("input[type=button]", 5).Click();
            Assert.False(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            AssertUI.AlertTextEquals(browser, "Conditional 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            Assert.False(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            AssertUI.AlertTextEquals(browser, "Conditional 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            //localization - resource binding in confirm postback handler message

            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "EnglishValue");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "7");

            browser.First("#ChangeLanguageCZ").Click();

            browser.WaitFor(() => {
                index = browser.First("[data-ui=\"command-index\"]");
                AssertUI.InnerTextEquals(index, "0");
            }, 1500, "Redirect to CZ localization failed.");

            section = browser.First(sectionSelector);

            //ChangeLanguageEN
            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "CzechValue");
            browser.DismissAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "0");

            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "CzechValue");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "7");
        }

        [Fact]
        public void Feature_PostBack_SuppressPostBackHandler()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_SuppressPostBackHandler);

                var counter = browser.First("span[data-ui=counter]");
                AssertUI.InnerTextEquals(counter, "0");

                browser.First("input[data-ui=static-suppress-value]").Click();
                AssertUI.InnerTextEquals(counter, "0");

                browser.First("input[data-ui=multiple-suppress-handlers]").Click();
                AssertUI.InnerTextEquals(counter, "0");

                var conditionValue = browser.First("span[data-ui=condition]");
                AssertUI.InnerTextEquals(conditionValue, "true");

                browser.First("input[data-ui=value-binding-suppress]").Click();
                AssertUI.InnerTextEquals(counter, "0");

                browser.First("input[data-ui=change-condition]").Click();
                AssertUI.InnerTextEquals(conditionValue, "false");

                browser.First("input[data-ui=value-binding-suppress]").Click();
                AssertUI.InnerTextEquals(counter, "1");
            });
        }

        [Fact]
        public void Feature_PostBack_PostBackHandlerCommandTypes()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlerCommandTypes);

                var counter = browser.First("span.result");
                AssertUI.InnerTextEquals(counter, "0");

                // command: success
                var button = browser.ElementAt("input[type=button]", 0);
                button.Click();
                AssertUI.HasClass(button, "pending");
                browser.WaitFor(() => {
                    AssertUI.HasClass(button, "success");
                    AssertUI.InnerTextEquals(counter, "1");
                }, 5000);

                // command: client validation
                button = browser.ElementAt("input[type=button]", 1);
                button.Click();
                AssertUI.HasClass(button, "error");
                AssertUI.InnerTextEquals(counter, "1");

                browser.Wait(1000);
                browser.Single("#debugNotification").Click();

                // command: server validation
                button = browser.ElementAt("input[type=button]", 2);
                button.Click();
                AssertUI.HasClass(button, "pending");
                browser.WaitFor(() => {
                    AssertUI.HasClass(button, "success"); // TODO: we should change the behavior so server-side validation will reject the promise
                    AssertUI.InnerTextEquals(counter, "1");
                }, 5000);

                // command: server exception
                button = browser.ElementAt("input[type=button]", 3);
                button.Click();
                AssertUI.HasClass(button, "pending");
                browser.WaitFor(() => {
                    AssertUI.HasClass(button, "error");
                    AssertUI.InnerTextEquals(counter, "1");
                }, 5000);
                
                browser.Single("#closeDebugWindow").Click();

                // staticCommand server call: success
                button = browser.ElementAt("input[type=button]", 4);
                button.Click();
                AssertUI.HasClass(button, "pending");
                browser.WaitFor(() => {
                    AssertUI.HasClass(button, "success");
                    AssertUI.InnerTextEquals(counter, "2");
                }, 5000);

                // staticCommand server call: server exception
                button = browser.ElementAt("input[type=button]", 5);
                button.Click();
                AssertUI.HasClass(button, "pending");
                browser.WaitFor(() => {
                    AssertUI.HasClass(button, "error");
                    AssertUI.InnerTextEquals(counter, "2");
                }, 5000);

                browser.Single("#closeDebugWindow").Click();

                // staticCommand local-only action: success
                button = browser.ElementAt("input[type=button]", 6);
                button.Click();
                AssertUI.HasClass(button, "success");
                AssertUI.InnerTextEquals(counter, "3");
            });
        }

        public PostBackTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
