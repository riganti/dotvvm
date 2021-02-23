using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class ChangedEventTests : AppSeleniumTest
    {
        public ChangedEventTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        public void Complex_ChangedEvent_ChangedEvent()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ChangedEvent_ChangedEvent);

                var totalChanges = browser.First("*[data-id='total-changes']");
                AssertUI.InnerTextEquals(totalChanges, "0");

                // first textbox with update mode on key press
                var textBox1 = browser.First("input[type=text]");
                textBox1.SetFocus();

                new Actions(browser.Driver).SendKeys("test").Perform();
                AssertUI.InnerTextEquals(totalChanges, "0");

                var firstTextbox = browser.First("*[data-id='first-textbox']");
                browser.WaitFor(() => {
                    AssertUI.InnerText(firstTextbox, s => s.Contains("Valuetes"));
                }, 4000, 100);

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(firstTextbox, "Valuetest");
                }, 4000, 100);
                AssertUI.InnerTextEquals(totalChanges, "1");

                // second textbox
                var textBox2 = browser.ElementAt("input[type=text]", 1);
                browser.FireJsBlur();
                textBox2.SetFocus();
                new Actions(browser.Driver).SendKeys("test").Perform();

                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "1");
                }, 4000, 100);
                var secondTextbox = browser.First("*[data-id='second-textbox']");
                AssertUI.InnerTextEquals(secondTextbox, "Value");

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(secondTextbox, "Valuetest");
                }, 4000, 100);

                AssertUI.InnerTextEquals(totalChanges, "2");

                // third textbox
                var textBox3 = browser.ElementAt("input[type=text]", 2);
                browser.FireJsBlur();
                textBox3.SetFocus();
                new Actions(browser.Driver).SendKeys("a").Perform();
                browser.Wait(100);
                new Actions(browser.Driver).SendKeys("b").Perform();
                browser.Wait(100);
                new Actions(browser.Driver).SendKeys("c").Perform();
                browser.Wait(100);
                new Actions(browser.Driver).SendKeys(Keys.Backspace).Perform();
                browser.Wait(100);

                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "6");
                }, 4000, 100);
                var thirdTextBox = browser.First("*[data-id='third-textbox']");
                AssertUI.InnerTextEquals(thirdTextBox, "ab");

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(thirdTextBox, "ab");
                }, 4000, 100);

                AssertUI.InnerTextEquals(totalChanges, "6");

                // click on checkbox
                browser.Click("input[type=checkbox]");
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "7");
                }, 4000, 100);

                browser.Click("input[type=checkbox]");
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "8");
                }, 4000, 100);

                // click on radio button
                browser.ElementAt("input[type=radio]", 0).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "9");
                }, 4000, 100);

                browser.ElementAt("input[type=radio]", 1).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "10");
                }, 4000, 100);

                browser.ElementAt("input[type=radio]", 2).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "11");
                }, 4000, 100);

                browser.ElementAt("input[type=radio]", 3).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "12");
                }, 4000, 100);

                browser.ElementAt("input[type=radio]", 4).Click();
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "13");
                }, 4000, 100);

                // combo box
                browser.First("select").Select(1);
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "14");
                }, 4000, 100);
                browser.First("select").Select(2);
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "15");
                }, 4000, 100);

                browser.First("select").Select(0);
                browser.WaitFor(() => {
                    AssertUI.InnerTextEquals(totalChanges, "16");
                }, 4000, 100);

            });
        }


    }
}
