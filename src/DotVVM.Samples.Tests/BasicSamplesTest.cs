using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Interactions;

namespace DotVVM.Samples.Tests
{
    public abstract class BasicSamplesTests : SeleniumTestBase
    {
        protected abstract string BaseUrl { get; }

        private const int WaitTime = 1200;

        public void Sample1Test(string sampleUrl = "Sample1")
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + sampleUrl);
                Thread.Sleep(WaitTime);

                Assert.AreEqual(3, browser.FindAll(".table tr").Count);

                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);

                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));
            });
        }

        public void Sample2Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample2");

                var boxes = browser.FindAll("fieldset");

                // single check box
                boxes[0].Find("input[type=checkbox]").Click();
                boxes[0].Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("True", boxes[0].Find("span").GetText());

                // check box list
                boxes[1].FindAll("input[type=checkbox]")[1].Click();
                boxes[1].FindAll("input[type=checkbox]")[2].Click();
                boxes[1].Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("g, b", boxes[1].Find("span").GetText());
                boxes[1].FindAll("input[type=checkbox]")[2].Click();
                boxes[1].FindAll("input[type=checkbox]")[0].Click();
                boxes[1].Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("g, r", boxes[1].Find("span").GetText());

                // radion button list
                boxes[2].FindAll("input[type=radio]")[2].Click();
                boxes[2].FindAll("input[type=radio]")[3].Click();
                boxes[2].Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("4", boxes[2].FindAll("span").Last().GetText());
                boxes[2].FindAll("input[type=radio]")[1].Click();
                boxes[2].Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("2", boxes[2].FindAll("span").Last().GetText());

                // checked changed
                boxes[3].FindAll("input[type=checkbox]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", boxes[3].FindAll("span").Last().GetText());
                Assert.IsTrue(boxes[3].FindAll("input[type=checkbox]:checked").Any());
                boxes[3].FindAll("input[type=checkbox]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("2", boxes[3].FindAll("span").Last().GetText());
                Assert.IsFalse(boxes[3].FindAll("input[type=checkbox]:checked").Any());
            });
        }

        public void Sample3Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample3");
                var table = browser.Find(".table");

                // add lines
                var addButton = browser.FindAll("a").Single(l => l.GetText().Contains("Add"));
                addButton.Click();
                Thread.Sleep(WaitTime);
                addButton.Click();
                Thread.Sleep(WaitTime);
                addButton.Click();
                Thread.Sleep(WaitTime);

                // fill second line
                var cells = table.FindAll("tr")[2].FindAll("td");
                cells[0].Find("input").Clear();
                cells[0].Find("input").SendKeys("111");
                cells[1].Find("select").Select(1);
                cells[2].Find("input").Clear();
                cells[2].Find("input").SendKeys("Bread");
                cells[3].Find("input").Clear();
                cells[3].Find("input").SendKeys("12");
                cells[4].Find("input").Clear();
                cells[4].Find("input").SendKeys("10");

                // fill third line
                cells = table.FindAll("tr")[3].FindAll("td");
                cells[0].Find("input").Clear();
                cells[0].Find("input").SendKeys("222");
                cells[1].Find("select").Select(2);
                cells[2].Find("input").Clear();
                cells[2].Find("input").SendKeys("Ham");
                cells[3].Find("input").Clear();
                cells[3].Find("input").SendKeys("1");
                cells[4].Find("input").Clear();
                cells[4].Find("input").SendKeys("5");

                // fill fourth line
                cells = table.FindAll("tr")[4].FindAll("td");
                cells[0].Find("input").Clear();
                cells[0].Find("input").SendKeys("333");
                cells[1].Find("select").Select(3);
                cells[2].Find("input").Clear();
                cells[2].Find("input").SendKeys("Cheese");
                cells[3].Find("input").Clear();
                cells[3].Find("input").SendKeys("10");
                cells[4].Find("input").Clear();
                cells[4].Find("input").SendKeys("15");

                // verify line totals
                browser.Find("input[type=text]").Click();
                Thread.Sleep(WaitTime * 5);
                Assert.AreEqual("126", table.FindAll("tr")[2].FindAll("td")[5].GetText().Trim());
                Assert.AreEqual("5.5", table.FindAll("tr")[3].FindAll("td")[5].GetText().Trim());
                Assert.AreEqual("180", table.FindAll("tr")[4].FindAll("td")[5].GetText().Trim());

                // recalculate
                var recalcButton = browser.FindAll("a").Single(l => l.GetText().Contains("Recalc"));
                recalcButton.Click();
                Thread.Sleep(WaitTime);

                // verify total price
                Assert.AreEqual("407.5", table.FindAll("tr").Last().FindAll("th")[1].GetText().Trim());

                // remove second line
                table.FindAll("tr")[2].FindAll("td").Last().Find("a").Click();
                Thread.Sleep(WaitTime);

                // verify total price
                Assert.AreEqual("281.5", table.FindAll("tr").Last().FindAll("th")[1].GetText().Trim());
            });
        }

        public void Sample4Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample4");

                Assert.AreEqual(3, browser.FindAll(".table tr").Count);

                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);

                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));
            });
        }

        public void Sample5Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample5");

                Assert.AreEqual("15", browser.FindAll("input[type=text]")[0].GetAttribute("value"));
                Assert.AreEqual("25", browser.FindAll("input[type=text]")[1].GetAttribute("value"));

                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                browser.FindAll("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                browser.FindAll("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("16", browser.FindAll("input[type=text]")[0].GetAttribute("value"));
                Assert.AreEqual("27", browser.FindAll("input[type=text]")[1].GetAttribute("value"));
            });
        }

        public void Sample6Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample6");

                // get original value
                var originalValue = browser.Find("strong span").GetText();

                // modify protected data
                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                // make sure it happened
                Assert.AreEqual("hello", browser.Find("strong span").GetText());

                // try to do postback
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // verify that the original value was restored
                Assert.AreEqual(originalValue, browser.Find("strong span").GetText());
            });
        }

        public void Sample8Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample8");

                // init alert
                Thread.Sleep(WaitTime);
                Assert.AreEqual("init", browser.GetAlertText());
                browser.ConfirmAlert();

                // postback alerts
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("beforePostback", browser.GetAlertText());
                browser.ConfirmAlert();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("afterPostback", browser.GetAlertText());
                browser.ConfirmAlert();

                // error alerts
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("beforePostback", browser.GetAlertText());
                browser.ConfirmAlert();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("custom error handler", browser.GetAlertText());
                browser.ConfirmAlert();
            });
        }

        public void Sample9Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample9");

                Assert.AreEqual("This comes from resource file!", browser.Find("p").GetText().Trim());

                // change language
                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("Tohle pochází z resource souboru!", browser.Find("p").GetText().Trim());
            });
        }

        public void Sample10Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample10");
                Thread.Sleep(WaitTime);

                var originalUrl = browser.CurrentUrl;
                Assert.IsTrue(originalUrl.Contains("?time="));

                // click the button
                browser.Find("input[type=button]").Click();
                Thread.Sleep(WaitTime);

                Assert.IsTrue(originalUrl.Contains("?time="));
                Assert.AreNotEqual(originalUrl, browser.CurrentUrl);
            });
        }

        public void Sample11Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample11");

                // ensure validators not visible
                Assert.AreEqual(0, browser.FindAll("li").Count);
                Assert.IsFalse(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindAll("span")[2].IsDisplayed());

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.AreEqual(1, browser.FindAll("li").Count);
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsTrue(browser.FindAll("span")[2].IsDisplayed());

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(1, browser.FindAll("li").Count);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.AreEqual(1, browser.FindAll("li").Count);
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsTrue(browser.FindAll("span")[2].IsDisplayed());

                // fill valid value in the task title
                browser.Clear("input[type=text]");
                browser.SendKeys("input[type=text]", "_test@mail.com");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators not visible
                Assert.AreEqual(0, browser.FindAll("li").Count);
                Assert.IsFalse(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindAll("span")[2].IsDisplayed());

                // ensure the item was added
                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                // TODO: ensure items starting with underscore can't be done
            });
        }

        public void Sample12Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample12");

                // enter number of lines and click the button
                browser.Clear("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(14, browser.FindAll("br").Count);

                // change number of lines and click the button
                browser.Clear("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(4, browser.FindAll("br").Count);
            });
        }

        public void Sample13Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample13");

                // select city
                browser.FindAll("select")[0].Select(1);
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);

                // select hotel
                browser.FindAll("select")[1].Select(1);
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("Hotel Seattle #2", browser.GetText("h2"));

                // select city
                browser.FindAll("select")[0].Select(0);
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);

                // select hotel
                browser.FindAll("select")[1].FindAll("option")[0].Click();
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("Hotel Prague #1", browser.GetText("h2"));
            });
        }

        public void Sample14Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample14");

                // ensure month names are rendered on the server
                Assert.AreEqual(0, browser.Find("table tr td").FindAll("span").Count);

                // fill textboxes
                for (int i = 0; i < 12; i++)
                {
                    browser.FindAll("input[type=text]")[i].Clear();
                    browser.FindAll("input[type=text]")[i].SendKeys((i + 1).ToString());
                }
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime * 2);

                // validate result
                Assert.AreEqual("78", browser.Last("span").GetText().Trim());
            });
        }

        public void Sample15Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample15");

                // try the long action interrupted by the short one
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(2000);
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);

                // the postback index should be 1 now (because of short action)
                Assert.AreEqual("1", browser.FindAll("span")[0].GetText().Trim());
                Assert.AreEqual("short", browser.FindAll("span")[1].GetText().Trim());

                // the result of the long action should be canceled, the counter shouldn't increase
                Thread.Sleep(10000);
                Assert.AreEqual("1", browser.FindAll("span")[0].GetText().Trim());
                Assert.AreEqual("short", browser.FindAll("span")[1].GetText().Trim());
                Thread.Sleep(WaitTime);

                // test update progress control
                Assert.IsFalse(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
                browser.FindAll("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
                Thread.Sleep(6000);
                Assert.IsFalse(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
            });
        }

        public void Sample16Test(string sampleUrl = "Sample16")
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + sampleUrl);

                Action performTest = () =>
                {
                    // make sure that third row's first cell is yellow
                    Assert.AreEqual("", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetAttribute("class"));
                    Assert.AreEqual("alternate", browser.FindAll("table")[0].FindAll("tr")[2].FindAll("td")[0].GetAttribute("class"));

                    // go to second page
                    Assert.AreEqual("1", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());
                    browser.FindAll("ul")[0].FindAll("li a").Single(a => a.GetText() == "2").Click();
                    Thread.Sleep(WaitTime);

                    // go to previous page
                    Assert.AreEqual("11", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());
                    browser.FindAll("ul")[0].FindAll("li a").Single(a => a.GetText() == "««").Click();
                    Thread.Sleep(WaitTime);

                    // go to next page
                    Assert.AreEqual("1", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());
                    browser.FindAll("ul")[0].FindAll("li a").Single(a => a.GetText() == "»»").Click();
                    Thread.Sleep(WaitTime);

                    // try the disabled link - nothing should happen
                    Assert.AreEqual("11", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());
                    browser.FindAll("ul")[0].FindAll("li a").Single(a => a.GetText() == "»»").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("11", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());

                    // try sorting in the first grid
                    browser.FindAll("table")[0].FindAll("tr")[0].FindAll("th")[2].Find("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("4", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());

                    // sort descending in the first grid
                    browser.FindAll("table")[0].FindAll("tr")[0].FindAll("th")[2].Find("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("9", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());

                    // sort by different column in the first grid
                    browser.FindAll("table")[0].FindAll("tr")[0].FindAll("th")[0].Find("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("1", browser.FindAll("table")[0].FindAll("tr")[1].FindAll("td")[0].GetText());

                    // try sorting in the first grid
                    browser.FindAll("table")[1].FindAll("tr")[0].FindAll("th")[2].Find("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("4", browser.FindAll("table")[1].FindAll("tr")[1].FindAll("td")[0].GetText());

                    // sort by different column in the first grid
                    browser.FindAll("table")[1].FindAll("tr")[0].FindAll("th")[0].Find("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("1", browser.FindAll("table")[1].FindAll("tr")[1].FindAll("td")[0].GetText());
                };

                Thread.Sleep(WaitTime * 6);
                performTest();
                Thread.Sleep(WaitTime * 6);
                browser.NavigateToUrl(BaseUrl);
                Thread.Sleep(WaitTime * 6);
                browser.NavigateBack();
                Thread.Sleep(WaitTime * 6);
                performTest();
            });
        }

        public void Sample17Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample17");
                Thread.Sleep(WaitTime);

                // make sure the default page loads
                Thread.Sleep(WaitTime);
                Assert.IsFalse(string.IsNullOrWhiteSpace(browser.GetAlertText()));
                browser.ConfirmAlert();
                Assert.AreEqual(browser.CurrentUrl, BaseUrl + "Sample17#!/Sample17/B");

                // go to first page
                browser.Click("a");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/A/16", browser.CurrentUrl);

                // test first page
                Assert.AreEqual("0", browser.FindAll("span")[1].GetText());
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("3", browser.FindAll("span")[1].GetText());

                // go to second page
                browser.FindAll("a").Single(l => l.GetText().Contains("Go to Task List")).Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/B", browser.CurrentUrl);

                // try the task list
                Assert.AreEqual(3, browser.FindAll(".table tr").Count);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(4, browser.FindAll(".table tr").Count);
                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));

                // test the browse back button
                browser.NavigateBack();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/A/16", browser.CurrentUrl);

                // test first page
                Assert.AreEqual("0", browser.FindAll("span")[1].GetText());
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("3", browser.FindAll("span")[1].GetText());

                // test the forward button
                browser.NavigateForward();
                Thread.Sleep(WaitTime);

                // test the redirect inside SPA
                browser.FindAll("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/A/15", browser.CurrentUrl);

                // test the redirect outside SPA
                browser.FindAll("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample1", browser.CurrentUrl.TrimEnd('/'));
            });
        }

        public void Sample18Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample18");

                // ensure validators not visible
                Assert.IsFalse(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindAll("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindAll(".summary1 li").Count);
                Assert.AreEqual(0, browser.FindAll(".summary2 li").Count);

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].GetAttribute("class").Contains("invalid"));
                Assert.IsTrue(browser.FindAll("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindAll(".summary1 li").Count);
                Assert.AreEqual(1, browser.FindAll(".summary2 li").Count);

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].GetAttribute("class").Contains("invalid"));
                Assert.IsTrue(browser.FindAll("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindAll(".summary1 li").Count);
                Assert.AreEqual(1, browser.FindAll(".summary2 li").Count);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators
                Assert.IsFalse(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindAll("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindAll("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindAll(".summary1 li").Count);
                Assert.AreEqual(0, browser.FindAll(".summary2 li").Count);
            });
        }

        public void Sample20Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample20");

                // click the validate button
                browser.FindAll("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);

                // ensure validators are hidden
                Assert.AreEqual("true", browser.FindAll("span").Last().GetText());
                Assert.AreEqual(0, browser.FindAll("li").Count());

                // load the customer
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // try to validate
                browser.FindAll("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(1, browser.FindAll("li").Count());
                Assert.IsTrue(browser.Find("li").GetText().Contains("Email"));

                // fix the e-mail address
                browser.FindAll("input[type=text]").Last().Clear();
                browser.FindAll("input[type=text]").Last().SendKeys("test@mail.com");
                browser.FindAll("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);

                // try to validate
                Assert.AreEqual("true", browser.FindAll("span").Last().GetText());
                Assert.AreEqual(0, browser.FindAll("li").Count());
            });
        }

        public void Sample21Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample21");

                Assert.AreEqual("0", browser.Find("*[data-id='total-changes']").GetText());

                // first textbox with update mode on key press
                var textBox1 = browser.FindAll("input[type=text]")[0];
                new Actions(browser.WebDriver).Click(textBox1.WebElement).Perform();
                Thread.Sleep(WaitTime);
                new Actions(browser.WebDriver).SendKeys("test").Perform();

                Thread.Sleep(WaitTime);
                Assert.AreEqual("0", browser.Find("*[data-id='total-changes']").GetText());
                Assert.AreEqual("Valuetest", browser.Find("*[data-id='first-textbox']").GetText());

                new Actions(browser.WebDriver).SendKeys(Keys.Tab).Perform();
                Assert.AreEqual("Valuetest", browser.Find("*[data-id='first-textbox']").GetText());
                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", browser.Find("*[data-id='total-changes']").GetText());

                // second textbox
                var textBox2 = browser.FindAll("input[type=text]")[1];
                new Actions(browser.WebDriver).Click(textBox2.WebElement).Perform();
                Thread.Sleep(WaitTime);
                new Actions(browser.WebDriver).SendKeys("test").Perform();

                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", browser.Find("*[data-id='total-changes']").GetText());
                Assert.AreEqual("Value", browser.Find("*[data-id='second-textbox']").GetText());

                new Actions(browser.WebDriver).SendKeys(Keys.Tab).Perform();
                Assert.AreEqual("Valuetest", browser.Find("*[data-id='second-textbox']").GetText());
                Thread.Sleep(WaitTime);
                Assert.AreEqual("2", browser.Find("*[data-id='total-changes']").GetText());


                // click on checkbox
                browser.Click("input[type=checkbox]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("3", browser.Find("*[data-id='total-changes']").GetText());

                browser.Click("input[type=checkbox]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("4", browser.Find("*[data-id='total-changes']").GetText());


                // click on radio button
                browser.FindAll("input[type=radio]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("5", browser.Find("*[data-id='total-changes']").GetText());

                browser.FindAll("input[type=radio]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("6", browser.Find("*[data-id='total-changes']").GetText());

                browser.FindAll("input[type=radio]")[2].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("7", browser.Find("*[data-id='total-changes']").GetText());

                browser.FindAll("input[type=radio]")[3].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("8", browser.Find("*[data-id='total-changes']").GetText());

                browser.FindAll("input[type=radio]")[4].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("9", browser.Find("*[data-id='total-changes']").GetText());


                // combo box
                browser.Find("select").Select(1);
                Thread.Sleep(WaitTime);
                Assert.AreEqual("10", browser.Find("*[data-id='total-changes']").GetText());

                browser.Find("select").Select(2);
                Thread.Sleep(WaitTime);
                Assert.AreEqual("11", browser.Find("*[data-id='total-changes']").GetText());

                browser.Find("select").Select(0);
                Thread.Sleep(WaitTime);
                Assert.AreEqual("12", browser.Find("*[data-id='total-changes']").GetText());
            });
        }

        public void Sample22Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample22");

                // verify link urls
                var urls = browser.FindAll("a").Select(a => a.GetAttribute("href")).ToList();

                Assert.AreEqual(BaseUrl + "Sample22/1", urls[0]);
                Assert.AreEqual(BaseUrl + "Sample22/2", urls[1]);
                Assert.AreEqual(BaseUrl + "Sample22/3", urls[2]);
                Assert.AreEqual(BaseUrl + "Sample22/1", urls[3]);
                Assert.AreEqual(BaseUrl + "Sample22/2", urls[4]);
                Assert.AreEqual(BaseUrl + "Sample22/3", urls[5]);
                Assert.AreEqual(BaseUrl + "Sample22/1", urls[6]);
                Assert.AreEqual(BaseUrl + "Sample22/2", urls[7]);
                Assert.AreEqual(BaseUrl + "Sample22/3", urls[8]);
                Assert.AreEqual(BaseUrl + "Sample22/1", urls[9]);
                Assert.AreEqual(BaseUrl + "Sample22/2", urls[10]);
                Assert.AreEqual(BaseUrl + "Sample22/3", urls[11]);

                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        StringAssert.IsNotNullOrWhiteSpace(browser.FindAll("a")[j * 6 + i].GetText());
                        StringAssert.IsNotNullOrWhiteSpace(browser.FindAll("a")[j * 6 + i + 3].GetText());
                    }
                }
            });
        }

        public void Sample24Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample24");

                // fill the values
                browser.FindAll("input[type=text]")[0].SendKeys("1");
                browser.FindAll("input[type=text]")[1].SendKeys("2");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // verify the results
                Assert.AreEqual("", browser.FindAll("input[type=text]")[0].GetAttribute("value"));
                Assert.AreEqual("2", browser.FindAll("input[type=text]")[1].GetAttribute("value"));
                Assert.AreEqual(",2", browser.FindAll("span").Last().GetText());
            });
        }

        public void Sample25Test()
        {
            var culture = new CultureInfo("cs-CZ");

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample25");

                // verify the first date
                browser.FindAll("input[type=text]")[0].Clear();
                browser.FindAll("input[type=text]")[0].SendKeys("18.2.1988");
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(new DateTime(1988, 2, 18), DateTime.Parse(browser.FindAll("span")[0].GetText()));
                browser.FindAll("input[type=text]")[0].Clear();
                browser.FindAll("input[type=text]")[0].SendKeys("test");
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(DateTime.MinValue, DateTime.Parse(browser.FindAll("span")[0].GetText()));

                // verify the second date
                browser.FindAll("input[type=text]")[1].Clear();
                browser.FindAll("input[type=text]")[1].SendKeys("2011-03-19 16:48:17");
                browser.FindAll("input[type=button]")[3].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(new DateTime(2011, 3, 19, 16, 48, 0),
                    DateTime.Parse(browser.FindAll("span")[1].GetText()));
                browser.FindAll("input[type=text]")[1].Clear();
                browser.FindAll("input[type=text]")[1].SendKeys("test");
                browser.FindAll("input[type=button]")[3].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("null", browser.FindAll("span")[1].GetText());

                // try to set dates from server
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                browser.FindAll("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindAll("input[type=text]")[0].GetAttribute("value"), culture)).TotalHours < 24); // there is no time in the field
                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindAll("input[type=text]")[1].GetAttribute("value"), culture)).TotalMinutes < 1); // the minutes can differ slightly
            });
        }

        public void Sample26Test(string sampleUrl = "Sample26")
        {
            RunInAllBrowsers(browser =>
            {
                Action<int, bool> checkValidator = (int field, bool visible) =>
                {
                    Assert.AreEqual(visible, browser.FindAll("span")[field * 4 + 0].IsDisplayed());
                    Assert.IsTrue(browser.FindAll("span")[field * 4 + 1].IsDisplayed());
                    Assert.AreEqual(visible,
                        browser.FindAll("span")[field * 4 + 1].GetAttribute("class").Contains("invalid"));
                    Assert.AreEqual(visible, browser.FindAll("span")[field * 4 + 2].IsDisplayed());
                    Assert.AreEqual(visible, !string.IsNullOrEmpty(browser.FindAll("span")[field * 4 + 3].GetAttribute("title")));
                };
                Action<int, int> checkSummary = (int field, int numberOfErrors) =>
                {
                    Assert.AreEqual(numberOfErrors, browser.FindAll(".summary" + (field + 1) + " li").Count);
                };

                browser.NavigateToUrl(BaseUrl + sampleUrl);

                // ensure validators hidden
                checkValidator(0, false);
                checkValidator(1, false);
                checkSummary(0, 0);
                checkSummary(1, 0);

                // leave both textboxes empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                checkValidator(0, true);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 2);

                // submit once again and test each validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                checkValidator(0, true);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 2);

                // fill invalid value in the task title
                browser.FindAll("input[type=text]")[0].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, false);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // clear value in the first field and verify that the second error appears
                browser.FindAll("input[type=text]")[0].Clear();
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, true);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 2);

                // fill the second field
                browser.FindAll("input[type=text]")[1].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, true);
                checkValidator(1, false);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // fill the first field
                browser.FindAll("input[type=text]")[0].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state - the first field won't be valid because there is also a server side rule
                checkValidator(0, false);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // fix the first field
                browser.FindAll("input[type=text]")[1].Clear();
                browser.FindAll("input[type=text]")[1].SendKeys("test@mail.com");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure everything is valid
                checkValidator(0, false);
                checkValidator(1, false);
                checkSummary(0, 0);
                checkSummary(1, 0);
            });
        }

        public void Sample27Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample27");

                var items1 = browser.FindAll(".list1 li");
                var items2 = browser.FindAll(".list2 li");

                Assert.AreEqual(items1[0].GetText(), items2[0].GetText());
                Assert.AreEqual(items1[1].GetText(), items2[1].GetText());
                Assert.AreEqual(items1[2].GetText(), items2[2].GetText());

                Assert.AreEqual(items1[4].GetText(), items2[4].GetText());
                Assert.AreEqual(items1[5].GetText(), items2[5].GetText());
                Assert.AreEqual(items1[6].GetText(), items2[6].GetText());

                Assert.AreEqual(items1[7].GetText(), items2[7].GetText());
                Assert.AreEqual(items1[8].GetText(), items2[8].GetText());
                Assert.AreEqual(items1[9].GetText(), items2[9].GetText());
                Assert.AreEqual(items1[10].GetText(), items2[10].GetText());

                Assert.AreEqual(items1[11].GetText(), items2[11].GetText());
                Assert.AreEqual(items1[12].GetText(), items2[12].GetText());
                Assert.AreEqual(items1[13].GetText(), items2[13].GetText());
            });
        }

        public void Sample28Test()
        {
            Sample16Test("Sample28");
        }

        public void Sample29Test()
        {
            Sample1Test("Sample29");
        }

        public void Sample30Test()
        {
            Sample26Test("Sample30");
        }

        public void Sample31Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample31");

                // select second option from combobox
                browser.Find("select").Click();
                Assert.IsNotNull(browser.Find("select").Find("option"));
                browser.Find("h1").Click();

                Thread.Sleep(WaitTime);

                Assert.AreEqual("1", browser.GetText("span"));

                // select second option from combobox
                browser.Find("select").Select(1);
                browser.Find("h1").Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("2", browser.GetText("span"));

                // select third option from combobox
                browser.Find("select").Select(2);
                browser.Find("h1").Click();

                Thread.Sleep(WaitTime);

                Assert.AreEqual("3", browser.GetText("span"));

                // select fourth option from combobox
                browser.Find("select").Select(3);
                browser.Find("h1").Click();

                Thread.Sleep(WaitTime);

                Assert.AreEqual("4", browser.GetText("span"));
            });
        }

        public void Sample32Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample32");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextProperty", "button");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextBinding", "button");
                CheckButtonTextIsSetAndTagName(browser, "#InputTextProperty", "input");
                CheckButtonTextIsSetAndTagName(browser, "#InputTextBinding", "input");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonInnerText", "button");

                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextPropertyUpperCase", "button");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextBindingUpperCase", "button");
                CheckButtonTextIsSetAndTagName(browser, "#InputTextPropertyUpperCase", "input");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonInnerTextUpperCase", "button");
            });
        }

        public void Sample33Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample33");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextProperty", "a");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonTextBinding", "a");
                CheckButtonTextIsSetAndTagName(browser, "#ButtonInnerText", "a");

                // try to click on a disabled button
                browser.Click("#EnabledLinkButton");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("0", browser.FindAll("span").Last().GetText());

                // enable it
                browser.Click("input[type=checkbox]");
                Thread.Sleep(WaitTime);
                browser.Click("#EnabledLinkButton");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", browser.FindAll("span").Last().GetText());

                // try to click on a disabled button again
                browser.Click("#EnabledLinkButton");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", browser.FindAll("span").Last().GetText());
            });
        }

        public void Sample34Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample34");
                CheckButtonTextIsSetAndTagName(browser, "#TextBox1", "input");
                CheckButtonTextIsSetAndTagName(browser, "#TextBox2", "input");
                CheckButtonTextIsSetAndTagName(browser, "#TextArea1", "textarea");
                CheckButtonTextIsSetAndTagName(browser, "#TextArea2", "textarea");
            });
        }

        private static void CheckButtonTextIsSetAndTagName(SeleniumBrowserHelper browser, string selector, string expectedTagName, string expectedValue = null, bool textCanBeNull = false)
        {
            // check tagName
            var element = browser.Find(selector);
            element.CheckTagName(expectedTagName);
            element.CheckTextValue(expectedValue, textCanBeNull);
        }

        public void Sample35Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample35");

                Assert.AreEqual(1, browser.FindAll("#part1>div").Count);
                Assert.AreEqual(4, browser.FindAll("#part1>div>p").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part1>div>p")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part1>div>p")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part1>div>p")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part1>div>p")[3].GetText());

                Assert.AreEqual(1, browser.FindAll("#part2>ul").Count);
                Assert.AreEqual(4, browser.FindAll("#part2>ul>li").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part2>ul>li")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part2>ul>li")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part2>ul>li")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part2>ul>li")[3].GetText());

                Assert.AreEqual(4, browser.FindAll("#part3>p").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part3>p")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part3>p")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part3>p")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part3>p")[3].GetText());

                Assert.AreEqual(1, browser.FindAll("#part1_server>div").Count);
                Assert.AreEqual(4, browser.FindAll("#part1_server>div>p").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part1_server>div>p")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part1_server>div>p")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part1_server>div>p")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part1_server>div>p")[3].GetText());

                Assert.AreEqual(1, browser.FindAll("#part2_server>ul").Count);
                Assert.AreEqual(4, browser.FindAll("#part2_server>ul>li").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part2_server>ul>li")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part2_server>ul>li")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part2_server>ul>li")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part2_server>ul>li")[3].GetText());

                Assert.AreEqual(4, browser.FindAll("#part3_server>p").Count);
                Assert.AreEqual("Test 1", browser.FindAll("#part3_server>p")[0].GetText());
                Assert.AreEqual("Test 2", browser.FindAll("#part3_server>p")[1].GetText());
                Assert.AreEqual("Test 3", browser.FindAll("#part3_server>p")[2].GetText());
                Assert.AreEqual("Test 4", browser.FindAll("#part3_server>p")[3].GetText());
            });
        }

        public void Sample36Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample36");
                Thread.Sleep(WaitTime);

                Assert.AreEqual("test1", browser.FindAll("*[data-id=test1_marker]").Single().GetAttribute("id"));
                Assert.AreEqual("test2", browser.FindAll("*[data-id=test2_marker]").Single().GetAttribute("id"));

                Assert.AreEqual("test1a", browser.FindAll("*[data-id=test1a_marker]").Single().GetAttribute("id"));
                Assert.AreEqual("test2a", browser.FindAll("*[data-id=test2a_marker]").Single().GetAttribute("id"));


                var control1 = browser.FindAll("#ctl1").Single();
                Assert.AreEqual("ctl1_control1", control1.FindAll("*[data-id=control1_marker]").Single().GetAttribute("id"));
                Assert.AreEqual("ctl1_control2", control1.FindAll("*[data-id=control2_marker]").Single().GetAttribute("id"));

                var control2 = browser.FindAll("#ctl2").Single();
                Assert.AreEqual("control1", control2.FindAll("*[data-id=control1_marker]").Single().GetAttribute("id"));
                Assert.AreEqual("control2", control2.FindAll("*[data-id=control2_marker]").Single().GetAttribute("id"));

                var repeater1 = browser.FindAll("*[data-id=repeater1]").Single();
                for (int i = 0; i < 4; i++)
                {
                    Assert.AreEqual(repeater1.GetAttribute("id") + "_i" + i + "_repeater1", repeater1.FindAll("*[data-id=repeater1_marker]")[i].GetAttribute("id"));
                    Assert.AreEqual(repeater1.GetAttribute("id") + "_i" + i + "_repeater2", repeater1.FindAll("*[data-id=repeater2_marker]")[i].GetAttribute("id"));
                }

                var repeater2 = browser.FindAll("*[data-id=repeater2]").Single();
                for (int i = 0; i < 4; i++)
                {
                    Assert.AreEqual(repeater2.GetAttribute("id") + "_i" + i + "_repeater1server", repeater2.FindAll("*[data-id=repeater1server_marker]")[i].GetAttribute("id"));
                    Assert.AreEqual(repeater2.GetAttribute("id") + "_i" + i + "_repeater2server", repeater2.FindAll("*[data-id=repeater2server_marker]")[i].GetAttribute("id"));
                }
            });
        }

        public void Sample40Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample40");
                Thread.Sleep(WaitTime);

                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(2, browser.FindAll("ul")[0].FindAll("li").Count);
                Assert.AreEqual("false", browser.Find("#result").GetText());

                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(1, browser.FindAll("ul")[1].FindAll("li").Count);
                Assert.AreEqual("false", browser.Find("#result").GetText());
            });
        }

        public void Sample43Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample43");
                Thread.Sleep(WaitTime);

                browser.FindAll("input")[0].SendKeys("25");
                Thread.Sleep(WaitTime);
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.IsFalse(browser.FindAll("span")[0].IsDisplayed());
                Assert.AreEqual("25", browser.FindAll("span")[1].GetText());

                browser.FindAll("input")[0].SendKeys("a");
                Thread.Sleep(WaitTime);
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.AreEqual("25", browser.FindAll("span")[1].GetText());
            });
        }

        public void Sample44Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample44");
                Thread.Sleep(WaitTime);

                Assert.IsTrue(browser.FindAll("select")[0].IsEnabled());
                Assert.IsTrue(browser.FindAll("input")[0].IsEnabled());
                Assert.IsTrue(browser.FindAll("label")[0].IsEnabled());
                Assert.IsTrue(browser.FindAll("label")[1].IsEnabled());
                Assert.IsTrue(browser.FindAll("label")[2].IsEnabled());
                Assert.IsTrue(browser.FindAll("select")[1].IsEnabled());

                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);


                Assert.IsFalse(browser.FindAll("select")[0].IsEnabled());
                Assert.IsFalse(browser.FindAll("input")[0].IsEnabled());
                browser.FindAll("label")[0].Click();
                browser.FindAll("label")[1].Click();
                browser.FindAll("label")[2].Click();
                Assert.IsFalse(browser.FindAll("label")[0].IsSelected());
                Assert.IsFalse(browser.FindAll("label")[1].IsSelected());
                Assert.IsFalse(browser.FindAll("label")[2].IsSelected());
                Assert.IsFalse(browser.FindAll("select")[1].IsEnabled());

            });

        }

        public void Sample45Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample45");
                Thread.Sleep(WaitTime);

                browser.FindAll("input")[0].SendKeys("hello");
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);

                // TODO: what's the point of this test?
            });
        }



        public void Sample46Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample46");
                Thread.Sleep(WaitTime);

                Action<string> isDisplayed = id => Assert.IsTrue(browser.IsDisplayed("#" + id));
                Action<string> isHidden = id => Assert.IsFalse(browser.IsDisplayed("#" + id));
                Action<string> isNotPresent = id => Assert.AreEqual(0, browser.FindAll("#" + id).Count);

                isHidden("marker1_parent");
                isDisplayed("marker1");

                isNotPresent("marker2_parent");
                isDisplayed("marker2");

                isHidden("marker3_parent");
                isDisplayed("marker3");

                isNotPresent("marker4_parent");
                isDisplayed("marker4");


                isDisplayed("nonempty_marker1_parent");
                isHidden("nonempty_marker1");

                isDisplayed("nonempty_marker2_parent");
                isNotPresent("nonempty_marker2");

                isDisplayed("nonempty_marker3_parent");
                isHidden("nonempty_marker3");

                isDisplayed("nonempty_marker4_parent");
                isNotPresent("nonempty_marker4");


                isHidden("null_marker1_parent");
                isDisplayed("null_marker1");

                isNotPresent("null_marker2_parent");
                isDisplayed("null_marker2");

                isHidden("null_marker3_parent");
                isDisplayed("null_marker3");

                isNotPresent("null_marker4_parent");
                isDisplayed("null_marker4");
            });
        }
    }
}