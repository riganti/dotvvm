using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.IO;

namespace Redwood.Samples.Tests
{
    public abstract class BasicSamplesTests : SeleniumTestBase
    {

        protected abstract string BaseUrl { get; }

        private const int WaitTime = 500;
        

        
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
                browser.SendKeys("input[type=text]", "Redwood rocks!");
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // verify that the original value was restored
                Assert.AreEqual(originalValue, browser.Find("strong span").GetText());
            });
        }

        
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
                browser.SendKeys("input[type=text]", "test@mail.com");
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
                browser.FindAll("select")[0].Click();
                browser.FindAll("select")[0].FindAll("option")[1].Click();
                browser.FindAll("input[type=button]")[0].Click();
                Thread.Sleep(WaitTime);

                // select hotel
                browser.FindAll("select")[1].FindAll("option")[1].Click();
                browser.FindAll("input[type=button]")[1].Click();
                Thread.Sleep(WaitTime);

                Assert.AreEqual("Hotel Seattle #2", browser.GetText("h2"));


                // select city
                browser.FindAll("select")[0].Click();
                browser.FindAll("select")[0].FindAll("option")[0].Click();
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
                Thread.Sleep(WaitTime);

                // validate result
                Assert.AreEqual("78", browser.FindAll("span").Last().GetText().Trim());
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
                Assert.IsFalse(browser.IsDisplayed("div[data-bind='redwoodUpdateProgressVisible: true']"));
                browser.FindAll("input[type=button]")[2].Click();
                Thread.Sleep(WaitTime);
                Assert.IsTrue(browser.IsDisplayed("div[data-bind='redwoodUpdateProgressVisible: true']"));
                Thread.Sleep(6000);
                Assert.IsFalse(browser.IsDisplayed("div[data-bind='redwoodUpdateProgressVisible: true']"));
            });
        }

        
        public void Sample16Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(BaseUrl + "Sample16");

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
                browser.SendKeys("input[type=text]", "Redwood rocks!");
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
                Assert.AreEqual(BaseUrl + "Sample1", browser.CurrentUrl);
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

                // leave textbox empty and submit the form
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

                // ensure validators visible
                Assert.IsTrue(browser.FindAll("span")[0].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].IsDisplayed());
                Assert.IsTrue(browser.FindAll("span")[1].GetAttribute("class").Contains("invalid"));
                Assert.IsTrue(browser.FindAll("span")[2].IsDisplayed());

                // submit once again and test the validation summary still holds one error
                browser.Click("input[type=button]");
                Thread.Sleep(WaitTime);

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
            });
        }
    }
}
