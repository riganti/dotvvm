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

        private const string baseUrl = "http://localhost:8628/";


        [TestMethod]
        public void Sample1Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(baseUrl + "Sample1");

                Assert.AreEqual(3, browser.FindAll(".table tr").Count);

                browser.SendKeys("input[type=text]", "Redwood rocks!");
                browser.Click("input[type=button]");

                Thread.Sleep(1000);

                Assert.AreEqual(4, browser.FindAll(".table tr").Count);

                browser.FindAll("a").Last().Click();
                Thread.Sleep(1000);

                Assert.IsTrue(browser.FindAll(".table tr").Last().GetAttribute("class").Contains("completed"));
            });
        }

    }
}
