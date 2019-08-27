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

                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "6");
                }, 1000, 100);
                var thirdTextBox = browser.First("*[data-id='third-textbox']");
                AssertUI.InnerTextEquals(thirdTextBox, "ab");

                new Actions(browser.Driver).SendKeys(Keys.Enter).SendKeys(Keys.Tab).Perform();
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(thirdTextBox, "ab");
                }, 1000, 100);

                AssertUI.InnerTextEquals(totalChanges, "6");

                // click on checkbox
                browser.Click("input[type=checkbox]");
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "7");
                }, 1000, 100);

                browser.Click("input[type=checkbox]");
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "8");
                }, 1000, 100);

                // click on radio button
                browser.ElementAt("input[type=radio]", 0).Click();
                AssertUI.InnerTextEquals(totalChanges, "9");

                browser.ElementAt("input[type=radio]", 1).Click();
                AssertUI.InnerTextEquals(totalChanges, "10");

                browser.ElementAt("input[type=radio]", 2).Click();
                AssertUI.InnerTextEquals(totalChanges, "11");

                browser.ElementAt("input[type=radio]", 3).Click();
                AssertUI.InnerTextEquals(totalChanges, "12");

                browser.ElementAt("input[type=radio]", 4).Click();
                AssertUI.InnerTextEquals(totalChanges, "13");

                // combo box
                browser.First("select").Select(1);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "14");
                }, 1000, 100);
                browser.First("select").Select(2);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "15");
                }, 1000, 100);

                browser.First("select").Select(0);
                browser.WaitFor(() =>
                {
                    AssertUI.InnerTextEquals(totalChanges, "16");
                }, 1000, 100);

            });
        }


    }
}
