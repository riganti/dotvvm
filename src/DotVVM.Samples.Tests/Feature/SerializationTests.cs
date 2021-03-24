using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class SerializationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_Serialization_Serialization()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_Serialization);

                // fill the values
                browser.ElementAt("input[type=text]", 0).SendKeys("1");
                browser.ElementAt("input[type=text]", 1).SendKeys("2");
                browser.Click("input[type=button]");

                // verify the results
                browser.WaitFor(() => {
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 0), "value", s => s.Equals(""));
                    AssertUI.Attribute(browser.ElementAt("input[type=text]", 1), "value", s => s.Equals("2"));
                    AssertUI.InnerTextEquals(browser.Last("span"), ",2");
                }, 2000);
            });
        }

        [Fact]
        public void Feature_Serialization_ObservableCollectionShouldContainObservables()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_ObservableCollectionShouldContainObservables);
                browser.Wait();

                // verify that the values are selected
                browser.ElementAt("select", 0).Select(0);
                browser.ElementAt("select", 1).Select(1);
                browser.ElementAt("select", 2).Select(2);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("p.result"), "1,2,3");
                    AssertUI.Attribute(browser.ElementAt("select", 0), "value", "1");
                    AssertUI.Attribute(browser.ElementAt("select", 1), "value", "2");
                    AssertUI.Attribute(browser.ElementAt("select", 2), "value", "3");
                    browser.Wait();
                }, 2000);

                // change the values
                browser.ElementAt("select", 0).Select(1);
                browser.ElementAt("select", 1).Select(2);
                browser.ElementAt("select", 2).Select(1);

                // click the button
                browser.Click("input[type=button]");

                // verify that the values are correct
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(browser.First("p.result"), "2,3,2");
                    AssertUI.Attribute(browser.ElementAt("select", 0), "value", "2");
                    AssertUI.Attribute(browser.ElementAt("select", 1), "value", "3");
                    AssertUI.Attribute(browser.ElementAt("select", 2), "value", "2");
                }, 2000);
            });
        }

        [Fact]
        public void Feature_Serialization_EnumSerializationWithJsonConverter()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_EnumSerializationWithJsonConverter);
                browser.Wait();

                // click on the button
                browser.Single("input[type=button]").Click().Wait();

                // make sure that deserialization worked correctly
                AssertUI.InnerTextEquals(browser.First("p.result"), "Success!");
            });
        }

        [Fact]
        public void Feature_Serialization_DeserializationVirtualElements()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_DeserializationVirtualElements);
                browser.Wait();

                // check that there are three rows
                browser.FindElements("thead tr").ThrowIfDifferentCountThan(2);
                browser.FindElements("tbody tr").ThrowIfDifferentCountThan(3);

                // add item
                browser.Single("input[type=text]").SendKeys("Four");
                browser.First("input[type=button]").Click().Wait();

                // check that there are four rows
                browser.FindElements("tbody tr").ThrowIfDifferentCountThan(4);
                AssertUI.InnerTextEquals(browser.ElementAt("tbody tr", 3).First("td"), "Four");

                // delete second row
                browser.ElementAt("tbody tr", 1).Single("input[type=button]").Click().Wait();

                // check that there are three rows
                browser.FindElements("tbody tr").ThrowIfDifferentCountThan(3);
                AssertUI.InnerTextEquals(browser.ElementAt("tbody tr", 0).First("td"), "One");
                AssertUI.InnerTextEquals(browser.ElementAt("tbody tr", 1).First("td"), "Three");
                AssertUI.InnerTextEquals(browser.ElementAt("tbody tr", 2).First("td"), "Four");
            });
        }

        [Fact]
        public void Feature_Serialization_Dictionary()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_Serialization_Dictionary);
                browser.WaitUntilDotvvmInited();

                var verifyButton = browser.First("verify", SelectByUiTestName);
                verifyButton.Click();

                var result = browser.First("result", SelectByUiTestName);
                AssertUI.TextEquals(result, "true", failureMessage: "Serialization of dictionary and List<KeyValuePair> does not work as expected.");
            });
        }


        public SerializationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
