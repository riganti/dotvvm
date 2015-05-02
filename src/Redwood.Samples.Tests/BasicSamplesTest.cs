using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.IO;

namespace Redwood.Samples.Tests
{
    [TestClass]
    public class BasicSamplesTests : SeleniumTestBase
    {

        private const string BaseUrl = "http://localhost:8628/";
        private const int WaitTime = 500;


        [TestMethod]
        public void Sample1Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample1");

                Assert.AreEqual(3, browser.FindAll(".table tr").Count);

                browser.SendKeys("input[type=text]", "Redwood rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);

                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));
            });
        }

        [TestMethod]
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
            });
        }

        [TestMethod]
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
                cells[1].Find("select").Click();
                cells[1].FindAll("select option")[1].Click();
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
                cells[1].Find("select").Click();
                cells[1].FindAll("select option")[2].Click();
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
                cells[1].Find("select").Click();
                cells[1].FindAll("select option")[3].Click();
                cells[2].Find("input").Clear();
                cells[2].Find("input").SendKeys("Cheese");
                cells[3].Find("input").Clear();
                cells[3].Find("input").SendKeys("10");
                cells[4].Find("input").Clear();
                cells[4].Find("input").SendKeys("15");

                // verify line totals
                browser.Find("input[type=text]").Click();
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

        [TestMethod]
        public void Sample4Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample4");

                Assert.AreEqual(3, browser.FindAll(".table tr").Count);

                browser.SendKeys("input[type=text]", "Redwood rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(WaitTime);

                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                browser.FindAll("a").Last().Click();
                Thread.Sleep(WaitTime);

                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));
            });
        }

        [TestMethod]
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

        [TestMethod]
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
                browser.SendKeys("input[type=text]", "Redwood rocks!");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // verify that the original value was restored
                Assert.AreEqual(originalValue, browser.Find("strong span").GetText());
            });
        }

        [TestMethod]
        public void Sample7Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample7");

                // select value in first calendar
                browser.FindAll("table")[0].FindAll("tr")[6].FindAll("td")[3].Find("a").Click();
                Thread.Sleep(WaitTime);

                // select value in the second calendar
                Assert.IsTrue(string.IsNullOrWhiteSpace(browser.FindAll("span").Last().GetText()));
                browser.FindAll("table")[1].FindAll("tr")[5].FindAll("td")[5].Find("a").Click();
                Thread.Sleep(WaitTime);
                Assert.IsFalse(string.IsNullOrWhiteSpace(browser.FindAll("span").Last().GetText()));

                // select value in third calendar
                browser.FindAll("table")[2].FindAll("tr")[4].FindAll("td")[1].Find("a").Click();
                Thread.Sleep(WaitTime);

                // verify that the selection is persisted
                Assert.IsTrue(browser.FindAll("table")[0].FindAll("tr")[6].FindAll("td")[3].GetAttribute("class").Contains("selected"));
            });
        }

        //[TestMethod]
        //public void Sample8Test()
        //{
        //    RunInAllBrowsers(browser =>
        //    {
        //        browser.NavigateToUrl(BaseUrl + "Sample8");

        //        // init alert
        //        Thread.Sleep(WaitTime);
        //        Assert.AreEqual("init", browser.GetAlertText());
        //        browser.ConfirmAlert();

        //        // postback alerts
        //        browser.FindAll("input[type=button]")[0].Click();
        //        Thread.Sleep(WaitTime);
        //        Assert.AreEqual("beforePostback", browser.GetAlertText());
        //        browser.ConfirmAlert();
        //        Thread.Sleep(WaitTime);
        //        Assert.AreEqual("afterPostback", browser.GetAlertText());
        //        browser.ConfirmAlert();

        //        // error alerts
        //        browser.FindAll("input[type=button]")[1].Click();
        //        Thread.Sleep(WaitTime);
        //        Assert.AreEqual("beforePostback", browser.GetAlertText());
        //        browser.ConfirmAlert();
        //        Thread.Sleep(WaitTime);
        //        Assert.AreEqual("error", browser.GetAlertText());
        //        browser.ConfirmAlert();
        //    });
        //}
    }
}
