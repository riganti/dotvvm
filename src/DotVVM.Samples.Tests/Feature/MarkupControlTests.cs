using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;


namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class MarkupControlTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_MarkupControl_CommandBindingInRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_CommandBindingInRepeater);
                browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Hello from DotVVM!");

                browser.ElementAt("input[type=button]", 0).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action1 - Item 1");
                }, 1000, 30);

                browser.ElementAt("input[type=button]", 1).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action2 - Item 1");
                }, 1000, 30);

                browser.ElementAt("input[type=button]", 2).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action1 - Item 2");
                }, 1000, 30);

                browser.ElementAt("input[type=button]", 3).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action2 - Item 2");
                }, 1000, 30);

                browser.ElementAt("input[type=button]", 4).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action1 - Item 3");
                }, 1000, 30);

                browser.ElementAt("input[type=button]", 5).Click();
                browser.WaitFor(() => {
                    browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("Action2 - Item 3");
                }, 1000, 30);
            });
        }

        [TestMethod]
        public void Feature_MarkupControl_CommandBindingInDataContextWithControlProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_CommandBindingInDataContextWithControlProperty);

                browser.First("span[data-uitest=result1]").CheckIfInnerTextEquals("Init");
                browser.First("span[data-uitest=result2]").CheckIfInnerTextEquals("Init");

                browser.ElementAt("input[type=button]", 0).Click().Wait();
                browser.First("span[data-uitest=result1]").CheckIfInnerTextEquals("changed");
                browser.First("span[data-uitest=result2]").CheckIfInnerTextEquals("Init");

                browser.ElementAt("input[type=button]", 1).Click().Wait();
                browser.First("span[data-uitest=result1]").CheckIfInnerTextEquals("changed");
                browser.First("span[data-uitest=result2]").CheckIfInnerTextEquals("changed");
            });
        }

        [TestMethod]
        public void Feature_MarkupControl_ControlPropertyUpdatedByServer()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyUpdatedByServer);

                browser.ElementAt("input[data-uitest=editor]", 0).CheckIfValue("false");
                browser.First("input[data-uitest=simpleProperty]").SendKeys("test");
                browser.First("input[data-uitest=simpleProperty]").SendKeys(Keys.Tab);
                browser.ElementAt("input[data-uitest=editor]", 0).CheckIfValue("true");
                browser.First("input[data-uitest=simpleProperty]").Clear().SendKeys("test2");
                browser.First("input[data-uitest=simpleProperty]").SendKeys(Keys.Tab);
                browser.ElementAt("input[data-uitest=editor]", 0).CheckIfValue("false");

                browser.ElementAt("input[data-uitest=editor]", 1).CheckIfValue("");
                browser.First("input[data-uitest=childProperty]").CheckIfValue("");
                browser.First("input[data-uitest=childPropertyButton]").Click().Wait();
                browser.ElementAt("input[data-uitest=editor]", 1).CheckIfValue("TEST");
                browser.First("input[data-uitest=childProperty]").CheckIfValue("TEST");
            });
        }

        [TestMethod]
        public void Feature_MarkupControl_ControlPropertyUpdating()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyUpdating);

                browser.ElementAt("input[type=text]", 0).CheckIfValue("TEST 123");
                browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("TEST 123 HUHA");
                browser.First("input[type=button]").Click().Wait();
                browser.ElementAt("input[type=text]", 0).CheckIfValue("ABC FFF");
                browser.First("span[data-uitest=result]").CheckIfInnerTextEquals("ABC FFF HUHA");
            });
        }

        [TestMethod]
        public void Feature_MarkupControl_ControlPropertyValidationPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_ControlPropertyValidationPage);

                browser.Single("input[type=button]").Click().Wait();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The Text field is required.");
                browser.Single("span").CheckIfInnerTextEquals("VALIDATION ERROR");

                browser.ElementAt("input[type=text]", 0).SendKeys("test");
                browser.Single("input[type=button]").Click().Wait();
                browser.ElementAt("input[type=text]", 1).CheckIfValue("test");
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The Text field is not a valid e-mail address.");
                browser.Single("span").CheckIfInnerTextEquals("VALIDATION ERROR");

                browser.ElementAt("input[type=text]", 0).SendKeys("@mail.com");
                browser.Single("input[type=button]").Click().Wait();
                browser.ElementAt("input[type=text]", 1).CheckIfValue("test@mail.com");
                browser.FindElements("li").ThrowIfDifferentCountThan(0);
                browser.Single("span").CheckIfInnerTextEquals("");

                browser.ElementAt("input[type=text]", 0).Clear();
                browser.Single("input[type=button]").Click().Wait();
                browser.FindElements("li").ThrowIfDifferentCountThan(1);
                browser.First("li").CheckIfInnerTextEquals("The Text field is required.");
                browser.Single("span").CheckIfInnerTextEquals("VALIDATION ERROR");
            });
        }

        [TestMethod]
        public void Feature_MarkupControl_MarkupControlRegistration()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_MarkupControlRegistration);

                browser.ElementAt("h2", 0).CheckIfInnerTextEquals("First Control");
                browser.ElementAt("h2", 1).CheckIfInnerTextEquals("Second control name was set from the binding");

                browser.ElementAt("input[type=text]", 0).CheckIfValue("15");
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                browser.ElementAt("input[type=text]", 0).CheckIfValue("16");
                browser.ElementAt("input[type=button]", 0).Click().Wait();
                browser.ElementAt("input[type=text]", 0).CheckIfValue("17");
                browser.ElementAt("input[type=button]", 1).Click().Wait();
                browser.ElementAt("input[type=text]", 0).CheckIfValue("16");

                browser.ElementAt("input[type=text]", 1).CheckIfValue("25");
                browser.ElementAt("input[type=button]", 2).Click().Wait();
                browser.ElementAt("input[type=text]", 1).CheckIfValue("26");
                browser.ElementAt("input[type=button]", 2).Click().Wait();
                browser.ElementAt("input[type=text]", 1).CheckIfValue("27");
                browser.ElementAt("input[type=button]", 3).Click().Wait();
                browser.ElementAt("input[type=text]", 1).CheckIfValue("26");
            });
        }


        [TestMethod]
        public void Feature_MarkupControl_MultiControlHierarchy()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_MarkupControl_MultiControlHierarchy);

                var ul = browser.First("ul", By.CssSelector);
                ul.FindElements("li", By.CssSelector).ThrowIfDifferentCountThan(20);
            });
        }
    }
}
