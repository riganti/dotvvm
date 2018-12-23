using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;


namespace DotVVM.Samples.Tests.New.Complex
{
    public class ChangedEventTests : AppSeleniumTest
    {
        public ChangedEventTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void Complex_ChangedEvent_ChangedEvent()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ChangedEvent_ChangedEvent);

                var totalChanges = browser.First("*[data-id='total-changes']");
                AssertUI.InnerTextEquals(totalChanges, "0");

                // first textbox with update mode on key press
                var textBox1 = browser.First("input[type=text]");
                textBox1.SetFocus();

                new Actions(browser.Driver).SendKeys("test").Perform();
                AssertUI.InnerTextEquals(totalChanges, "0");

                var firstTextbox = browser.First("*[data-id='first-textbox']");
                browser.WaitFor(() =>
                {
                    AssertUI.InnerText(firstTextbox, s=> s.Contains("Valuetes"));
                }, 1000, 100);

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(firstTextbox, "Valuetest");
                }, 1000, 100);
                AssertUI.InnerTextEquals(totalChanges, "1");

                // second textbox
                var textBox2 = browser.ElementAt("input[type=text]", 1);
                browser.FireJsBlur();
                textBox2.SetFocus();
                new Actions(browser.Driver).SendKeys("test").Perform();

                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "1");
                }, 1000, 100);
                var secondTextbox = browser.First("*[data-id='second-textbox']");
                AssertUI.InnerTextEquals(secondTextbox, "Value");

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(secondTextbox, "Valuetest");
                }, 1000, 100);

                AssertUI.InnerTextEquals(totalChanges, "2");

                // click on checkbox
                browser.Click("input[type=checkbox]");
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "3");
                }, 1000, 100);

                browser.Click("input[type=checkbox]");
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "4");
                }, 1000, 100);

                // click on radio button
                browser.ElementAt("input[type=radio]", 0).Click();
                AssertUI.InnerTextEquals(totalChanges, "5");

                browser.ElementAt("input[type=radio]", 1).Click();
                AssertUI.InnerTextEquals(totalChanges, "6");

                browser.ElementAt("input[type=radio]", 2).Click();
                AssertUI.InnerTextEquals(totalChanges, "7");

                browser.ElementAt("input[type=radio]", 3).Click();
                AssertUI.InnerTextEquals(totalChanges, "8");

                browser.ElementAt("input[type=radio]", 4).Click();
                AssertUI.InnerTextEquals(totalChanges, "9");

                // combo box
                browser.First("select").Select(1);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "10");
                }, 1000, 100);
                browser.First("select").Select(2);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "11");
                }, 1000, 100);

                browser.First("select").Select(0);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "12");
                }, 1000, 100);

            });
        }


    }
}
