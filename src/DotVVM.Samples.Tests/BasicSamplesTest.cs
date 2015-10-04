using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests
{
    public abstract class BasicSamplesTests : SeleniumTestBase
    {
        protected abstract string BaseUrl { get; }

        private const int WaitTime = 500;

        public void Sample1Test(string sampleUrl = "Sample1")
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + sampleUrl);
                Thread.Sleep(WaitTime);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);

                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);

                browser.Last("a").Click();
                Thread.Sleep(WaitTime);

                browser.Last(".table tr").CheckAttribute("class", a => a.Contains("completed"));
            });
        }

        public void Sample2Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample2");

                var boxes = browser.FindElements("fieldset");

                // single check box
                boxes[0].First("input[type=checkbox]").Click();
                boxes[0].First("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("True", boxes[0].First("span").GetText());

                // check box list
                boxes[1].FindElements("input[type=checkbox]")[1].Click();
                boxes[1].FindElements("input[type=checkbox]")[2].Click();
                boxes[1].First("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("g, b", boxes[1].First("span").GetText());
                boxes[1].FindElements("input[type=checkbox]")[2].Click();
                boxes[1].FindElements("input[type=checkbox]")[0].Click();
                boxes[1].First("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("g, r", boxes[1].First("span").GetText());

                // radion button list
                boxes[2].FindElements("input[type=radio]")[2].Click();
                boxes[2].FindElements("input[type=radio]")[3].Click();
                boxes[2].First("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("4", boxes[2].FindElements("span").Last().GetText());
                boxes[2].FindElements("input[type=radio]")[1].Click();
                boxes[2].First("input[type=button]").Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("2", boxes[2].FindElements("span").Last().GetText());

                // checked changed
                boxes[3].FindElements("input[type=checkbox]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("1", boxes[3].FindElements("span").Last().GetText());
                Assert.IsTrue(boxes[3].FindElements("input[type=checkbox]:checked").Any());
                boxes[3].FindElements("input[type=checkbox]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("2", boxes[3].FindElements("span").Last().GetText());
                Assert.IsFalse(boxes[3].FindElements("input[type=checkbox]:checked").Any());
            });
        }

        public void Sample3Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample3");
                var table = browser.First(".table");

                // add lines
                var addButton = browser.FindElements("a").Single(l => l.GetText().Contains("Add"));
                addButton.Click();
                Thread.Sleep(WaitTime);
                addButton.Click();
                Thread.Sleep(WaitTime);
                addButton.Click();
                Thread.Sleep(WaitTime);

                // fill second line
                var cells = table.FindElements("tr")[2].FindElements("td");
                cells[0].First("input").Clear();
                cells[0].First("input").SendKeys("111");
                cells[1].First("select").Select(1);
                cells[2].First("input").Clear();
                cells[2].First("input").SendKeys("Bread");
                cells[3].First("input").Clear();
                cells[3].First("input").SendKeys("12");
                cells[4].First("input").Clear();
                cells[4].First("input").SendKeys("10");

                // fill third line
                cells = table.FindElements("tr")[3].FindElements("td");
                cells[0].First("input").Clear();
                cells[0].First("input").SendKeys("222");
                cells[1].First("select").Select(2);
                cells[2].First("input").Clear();
                cells[2].First("input").SendKeys("Ham");
                cells[3].First("input").Clear();
                cells[3].First("input").SendKeys("1");
                cells[4].First("input").Clear();
                cells[4].First("input").SendKeys("5");

                // fill fourth line
                cells = table.FindElements("tr")[4].FindElements("td");
                cells[0].First("input").Clear();
                cells[0].First("input").SendKeys("333");
                cells[1].First("select").Select(3);
                cells[2].First("input").Clear();
                cells[2].First("input").SendKeys("Cheese");
                cells[3].First("input").Clear();
                cells[3].First("input").SendKeys("10");
                cells[4].First("input").Clear();
                cells[4].First("input").SendKeys("15");

                // verify line totals
                browser.First("input[type=text]").Click();
                Assert.AreEqual("126", table.FindElements("tr")[2].FindElements("td")[5].GetText().Trim());
                Assert.AreEqual("5.5", table.FindElements("tr")[3].FindElements("td")[5].GetText().Trim());
                Assert.AreEqual("180", table.FindElements("tr")[4].FindElements("td")[5].GetText().Trim());

                // recalculate
                var recalcButton = browser.FindElements("a").Single(l => l.GetText().Contains("Recalc"));
                recalcButton.Click();
                Thread.Sleep(WaitTime);

                // verify total price
                Assert.AreEqual("407.5", table.FindElements("tr").Last().FindElements("th")[1].GetText().Trim());

                // remove second line
                table.FindElements("tr")[2].FindElements("td").Last().First("a").Click();
                Thread.Sleep(WaitTime);

                // verify total price
                Assert.AreEqual("281.5", table.FindElements("tr").Last().FindElements("th")[1].GetText().Trim());
            });
        }

        public void Sample4Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.Browser.Manage().Window.Maximize();

                browser.NavigateToUrl(BaseUrl + "Sample4");
                Thread.Sleep(WaitTime);
                Thread.Sleep(WaitTime);
                Assert.AreEqual(3, browser.FindElements(".table tr").Count);

                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);
                Thread.Sleep(WaitTime);
                Assert.AreEqual(4, browser.FindElements(".table tr").Count);

                browser.FindElements("a").Last().Click();
                Thread.Sleep(WaitTime);
                Thread.Sleep(WaitTime);

                browser.Last(".table tr").CheckAttribute("class", a => a.Contains("completed"));
            });
        }

        public void Sample5Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample5");

                browser.ElementAt("input[type=text]", 0).CheckAttribute("value", "15");
                Assert.AreEqual("25", browser.FindElements("input[type=text]")[1].GetAttribute("value"));

                browser.FindElements("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                browser.FindElements("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                browser.FindElements("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("16", browser.FindElements("input[type=text]")[0].GetAttribute("value"));
                Assert.AreEqual("27", browser.FindElements("input[type=text]")[1].GetAttribute("value"));

                browser.ElementAt("input[type=text]", 1).CheckAttribute("value", "16");
            });
        }

        public void Sample6Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample6");
                Thread.Sleep(WaitTime);

                // get original value
                var originalValue = browser.First("strong span").GetText();

                // modify protected data
                browser.FindElements("a").Last().Click();
                Thread.Sleep(WaitTime);
                Thread.Sleep(WaitTime);

                // make sure it happened
                Assert.AreEqual("hello", browser.First("strong span").GetText());

                // try to do postback
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Thread.Sleep(WaitTime);

                // verify that the original value was restored
                Assert.AreEqual(originalValue, browser.First("strong span").GetText());
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
                browser.FindElements("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("beforePostback", browser.GetAlertText());
                browser.ConfirmAlert();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("afterPostback", browser.GetAlertText());
                browser.ConfirmAlert();

                // error alerts
                browser.FindElements("input[type=button]")[1].Click();
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

                Assert.AreEqual("This comes from resource file!", browser.First("p").GetText().Trim());

                // change language
                browser.FindElements("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("Tohle pochází z resource souboru!", browser.First("p").GetText().Trim());
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
                browser.First("input[type=button]").Click();
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
                Thread.Sleep(WaitTime);

                // ensure validators not visible
                browser.FindElements("li").ThrowIfDifferentCountThan(0);

                Assert.IsFalse(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindElements("span")[2].IsDisplayed());

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.AreEqual(1, browser.FindElements("li").Count);
                Assert.IsTrue(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsTrue(browser.FindElements("span")[2].IsDisplayed());

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(1, browser.FindElements("li").Count);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.AreEqual(1, browser.FindElements("li").Count);
                Assert.IsTrue(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsTrue(browser.FindElements("span")[2].IsDisplayed());

                // fill valid value in the task title
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test@mail.com");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators not visible
                Assert.AreEqual(0, browser.FindElements("li").Count);
                Assert.IsFalse(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindElements("span")[2].IsDisplayed());

                // ensure the item was added
                Assert.AreEqual(4, browser.FindElements(".table tr").Count);
            });
        }

        public void Sample12Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample12");

                // enter number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(14, browser.FindElements("br").Count);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(4, browser.FindElements("br").Count);
            });
        }

        public void Sample13Test()
        {
            //RunInAllBrowsers(browser =>
            //{
            //    browser.NavigateToUrl(BaseUrl + "Sample13");

            //    // select city
            //    browser.FindElements("select")[0].Select(1);
            //    browser.FindElements("input[type=button]")[0].Click();
            //    Thread.Sleep(WaitTime);

            //    // select hotel
            //    browser.FindElements("select")[1].Select(1);
            //    browser.FindElements("input[type=button]")[1].Click();
            //    Thread.Sleep(WaitTime);

            //    Assert.AreEqual("Hotel Seattle #2", browser.GetText("h2"));

            //    // select city
            //    browser.FindElements("select")[0].Select(0);
            //    browser.FindElements("input[type=button]")[0].Click();
            //    Thread.Sleep(WaitTime);

            //    // select hotel
            //    browser.FindElements("select")[1].FindElements("option")[0].Click();
            //    browser.FindElements("input[type=button]")[1].Click();
            //    Thread.Sleep(WaitTime);

            //    Assert.AreEqual("Hotel Prague #1", browser.GetText("h2"));
            //});
        }

        public void Sample14Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample14");

                // ensure month names are rendered on the server
                Assert.AreEqual(0, browser.First("table tr td").FindElements("span").Count);

                // fill textboxes
                for (int i = 0; i < 12; i++)
                {
                    browser.FindElements("input[type=text]")[i].Clear();
                    browser.FindElements("input[type=text]")[i].SendKeys((i + 1).ToString());
                }
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // validate result
                Assert.AreEqual("78", browser.Last("span").GetText().Trim());
            });
        }

        public void Sample15Test()
        {
            //    RunInAllBrowsers(browser =>
            //    {
            //        browser.NavigateToUrl(BaseUrl + "Sample15");

            //        // try the long action interrupted by the short one
            //        browser.FindElements("input[type=button]")[0].Click();
            //        Thread.Sleep(2000);
            //        browser.FindElements("input[type=button]")[1].Click();
            //        Thread.Sleep(WaitTime);

            //        // the postback index should be 1 now (because of short action)
            //        Assert.AreEqual("1", browser.FindElements("span")[0].GetText().Trim());
            //        Assert.AreEqual("short", browser.FindElements("span")[1].GetText().Trim());

            //        // the result of the long action should be canceled, the counter shouldn't increase
            //        Thread.Sleep(10000);
            //        Assert.AreEqual("1", browser.FindElements("span")[0].GetText().Trim());
            //        Assert.AreEqual("short", browser.FindElements("span")[1].GetText().Trim());
            //        Thread.Sleep(WaitTime);

            //        // test update progress control
            //        Assert.IsFalse(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
            //        browser.FindElements("input[type=button]")[2].Click();
            //        Thread.Sleep(WaitTime);
            //        Assert.IsTrue(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
            //        Thread.Sleep(6000);
            //        Assert.IsFalse(browser.IsDisplayed("div[data-bind='dotvvmUpdateProgressVisible: true']"));
            //    });
        }

        public void Sample16Test(string sampleUrl = "Sample16")
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + sampleUrl);

                Action performTest = () =>
                {
                    // make sure that thirs row's first cell is yellow
                    Assert.AreEqual("",
                        browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetAttribute("class"));
                    Assert.AreEqual("alternate",
                        browser.FindElements("table")[0].FindElements("tr")[2].FindElements("td")[0].GetAttribute("class"));

                    // go to second page
                    Assert.AreEqual("1", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());
                    browser.FindElements("ul")[0].FindElements("li a").Single(a => a.GetText() == "2").Click();
                    Thread.Sleep(WaitTime);

                    // go to previous page
                    Assert.AreEqual("11", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());
                    browser.FindElements("ul")[0].FindElements("li a").Single(a => a.GetText() == "««").Click();
                    Thread.Sleep(WaitTime);

                    // go to next page
                    Assert.AreEqual("1", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());
                    browser.FindElements("ul")[0].FindElements("li a").Single(a => a.GetText() == "»»").Click();
                    Thread.Sleep(WaitTime);

                    // try the disabled link - nothing should happen
                    Assert.AreEqual("11", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());
                    browser.FindElements("ul")[0].FindElements("li a").Single(a => a.GetText() == "»»").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("11", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());

                    // try sorting in the first grid
                    browser.FindElements("table")[0].FindElements("tr")[0].FindElements("th")[2].First("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("4", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());

                    // sort descending in the first grid
                    browser.FindElements("table")[0].FindElements("tr")[0].FindElements("th")[2].First("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("9", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());

                    // sort by different column in the first grid
                    browser.FindElements("table")[0].FindElements("tr")[0].FindElements("th")[0].First("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("1", browser.FindElements("table")[0].FindElements("tr")[1].FindElements("td")[0].GetText());

                    // try sorting in the first grid
                    browser.FindElements("table")[1].FindElements("tr")[0].FindElements("th")[2].First("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("4", browser.FindElements("table")[1].FindElements("tr")[1].FindElements("td")[0].GetText());

                    // sort by different column in the first grid
                    browser.FindElements("table")[1].FindElements("tr")[0].FindElements("th")[0].First("a").Click();
                    Thread.Sleep(WaitTime);
                    Assert.AreEqual("1", browser.FindElements("table")[1].FindElements("tr")[1].FindElements("td")[0].GetText());
                };

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
                Assert.AreEqual("0", browser.FindElements("span")[1].GetText());
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("3", browser.FindElements("span")[1].GetText());

                // go to second page
                browser.FindElements("a").Single(l => l.GetText().Contains("Go to Task List")).Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/B", browser.CurrentUrl);

                // try the task list
                Assert.AreEqual(3, browser.FindElements(".table tr").Count);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual(4, browser.FindElements(".table tr").Count);
                browser.FindElements("a").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue(browser.FindElements(".table tr").Last().GetAttribute("class").Contains("completed"));

                // test the browse back button
                browser.NavigateBack();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/A/16", browser.CurrentUrl);

                // test first page
                Assert.AreEqual("0", browser.FindElements("span")[1].GetText());
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);
                Assert.AreEqual("3", browser.FindElements("span")[1].GetText());

                // test the forward button
                browser.NavigateForward();
                Thread.Sleep(WaitTime);

                // test the redirect inside SPA
                browser.FindElements("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(BaseUrl + "Sample17#!/Sample17/A/15", browser.CurrentUrl);

                // test the redirect outside SPA
                browser.FindElements("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
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
                Assert.IsFalse(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindElements("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindElements(".summary1 li").Count);
                Assert.AreEqual(0, browser.FindElements(".summary2 li").Count);

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.IsTrue(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].GetAttribute("class").Contains("invalid"));
                Assert.IsTrue(browser.FindElements("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindElements(".summary1 li").Count);
                Assert.AreEqual(1, browser.FindElements(".summary2 li").Count);

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.IsTrue(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].GetAttribute("class").Contains("invalid"));
                Assert.IsTrue(browser.FindElements("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindElements(".summary1 li").Count);
                Assert.AreEqual(1, browser.FindElements(".summary2 li").Count);

                // fill invalid value in the task title
                browser.SendKeys("input[type=text]", "test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators
                Assert.IsFalse(browser.FindElements("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindElements("span")[1].IsDisplayed());
                Assert.IsFalse(browser.FindElements("span")[1].GetAttribute("class").Contains("validator"));
                Assert.IsFalse(browser.FindElements("span")[2].IsDisplayed());
                Assert.AreEqual(0, browser.FindElements(".summary1 li").Count);
                Assert.AreEqual(0, browser.FindElements(".summary2 li").Count);
            });
        }

        public void Sample20Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample20");

                // click the validate button
                browser.FindElements("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);

                // ensure validators are hidden
                Assert.AreEqual("true", browser.FindElements("span").Last().GetText());
                Assert.AreEqual(0, browser.FindElements("li").Count());

                // load the customer
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // try to validate
                browser.FindElements("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(1, browser.FindElements("li").Count());
                Assert.IsTrue(browser.First("li").GetText().Contains("Email"));

                // fix the e-mail address
                browser.FindElements("input[type=text]").Last().Clear();
                browser.FindElements("input[type=text]").Last().SendKeys("test@mail.com");
                browser.FindElements("input[type=button]").Last().Click();
                Thread.Sleep(WaitTime);

                // try to validate
                Assert.AreEqual("true", browser.FindElements("span").Last().GetText());
                Assert.AreEqual(0, browser.FindElements("li").Count());
            });
        }

        public void Sample22Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample22");
                Thread.Sleep(WaitTime);

                // verify link urls
                var urls = browser.FindElements("a").Select(a => a.GetAttribute("href")).ToList();

                for (int i = 0; i < 3; i++)
                {
                    Assert.AreEqual(urls[i], urls[i + 3]);

                    StringAssert.IsNotNullOrWhiteSpace(browser.FindElements("a")[i].GetText());
                    StringAssert.IsNotNullOrWhiteSpace(browser.FindElements("a")[i + 3].GetText());
                }
            });
        }

        public void Sample24Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample24");

                // fill the values
                browser.FindElements("input[type=text]")[0].SendKeys("1");
                browser.FindElements("input[type=text]")[1].SendKeys("2");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // verify the results
                Assert.AreEqual("", browser.FindElements("input[type=text]")[0].GetAttribute("value"));
                Assert.AreEqual("2", browser.FindElements("input[type=text]")[1].GetAttribute("value"));
                Assert.AreEqual(",2", browser.FindElements("span").Last().GetText());
            });
        }

        public void Sample25Test()
        {
            var culture = new CultureInfo("cs-CZ");

            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample25");

                // verify the first date
                browser.FindElements("input[type=text]")[0].Clear();
                browser.FindElements("input[type=text]")[0].SendKeys("18.2.1988");
                browser.FindElements("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(new DateTime(1988, 2, 18), DateTime.Parse(browser.FindElements("span")[0].GetText()));
                browser.FindElements("input[type=text]")[0].Clear();
                browser.FindElements("input[type=text]")[0].SendKeys("test");
                browser.FindElements("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(DateTime.MinValue, DateTime.Parse(browser.FindElements("span")[0].GetText()));

                // verify the second date
                browser.FindElements("input[type=text]")[1].Clear();
                browser.FindElements("input[type=text]")[1].SendKeys("2011-03-19 16:48:17");
                browser.FindElements("input[type=button]")[3].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual(new DateTime(2011, 3, 19, 16, 48, 0),
                    DateTime.Parse(browser.FindElements("span")[1].GetText()));
                browser.FindElements("input[type=text]")[1].Clear();
                browser.FindElements("input[type=text]")[1].SendKeys("test");
                browser.FindElements("input[type=button]")[3].Click();
                Thread.Sleep(WaitTime);
                Assert.AreEqual("null", browser.FindElements("span")[1].GetText());

                // try to set dates from server
                browser.FindElements("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);
                browser.FindElements("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindElements("input[type=text]")[0].GetAttribute("value"), culture)).TotalHours < 24); // there is no time in the field
                Assert.IsTrue((DateTime.Now - DateTime.Parse(browser.FindElements("input[type=text]")[1].GetAttribute("value"), culture)).TotalMinutes < 1); // the minutes can differ slightly
            });
        }

        public void Sample26Test(string sampleUrl = "Sample26")
        {
            RunInAllBrowsers(browser =>
            {
                Action<int, bool> checkValidator = (int field, bool visible) =>
                {
                    Assert.AreEqual(visible, browser.FindElements("span")[field * 4 + 0].IsDisplayed());
                    Assert.IsTrue(browser.FindElements("span")[field * 4 + 1].IsDisplayed());
                    Assert.AreEqual(visible,
                        browser.FindElements("span")[field * 4 + 1].GetAttribute("class").Contains("invalid"));
                    Assert.AreEqual(visible, browser.FindElements("span")[field * 4 + 2].IsDisplayed());
                    Assert.AreEqual(visible, !string.IsNullOrEmpty(browser.FindElements("span")[field * 4 + 3].GetAttribute("title")));
                };
                Action<int, int> checkSummary = (int field, int numberOfErrors) =>
                {
                    Assert.AreEqual(numberOfErrors, browser.FindElements(".summary" + (field + 1) + " li").Count);
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
                browser.FindElements("input[type=text]")[0].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, false);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // clear value in the first field and verify that the second error appears
                browser.FindElements("input[type=text]")[0].Clear();
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, true);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 2);

                // fill the second field
                browser.FindElements("input[type=text]")[1].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state
                checkValidator(0, true);
                checkValidator(1, false);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // fill the first field
                browser.FindElements("input[type=text]")[0].SendKeys("test");
                Thread.Sleep(WaitTime);
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validator state - the first field won't be valid because there is also a server side rule
                checkValidator(0, false);
                checkValidator(1, true);
                checkSummary(0, 0);
                checkSummary(1, 1);

                // fix the first field
                browser.FindElements("input[type=text]")[1].Clear();
                browser.FindElements("input[type=text]")[1].SendKeys("test@mail.com");
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

                var items1 = browser.FindElements(".list1 li");
                var items2 = browser.FindElements(".list2 li");

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
                Assert.IsNotNull(browser.First("select").First("option"));

                Thread.Sleep(WaitTime);
                browser.First("span").CheckIfInnerTextEquals("1");

                // select second option from combobox
                browser.First("select").Select(1);
                Thread.Sleep(WaitTime);
                browser.First("span").CheckIfInnerTextEquals("2");

                // select third option from combobox
                browser.First("select").Select(2);
                Thread.Sleep(WaitTime);
                browser.First("span").CheckIfInnerTextEquals("3");

                // select fourth option from combobox
                browser.First("select").Select(3);
                Thread.Sleep(WaitTime);
                browser.First("span").CheckIfInnerTextEquals("4");
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

        public void Sample35Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample35");

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

        private static void CheckButtonTextIsSetAndTagName(BrowserWrapper browser, string selector, string expectedTagName, string expectedValue = null)
        {
            // check tagName
            var element = browser.First(selector);
            element.CheckTagName(expectedTagName);
            if (expectedValue != null)
            {
                element.CheckIfInnerTextEquals(expectedValue);
            }
        }
    }
}