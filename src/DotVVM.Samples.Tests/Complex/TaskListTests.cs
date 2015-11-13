using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class AddTaskTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_TaskList()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("ComplexSamples/TaskList/TaskList");

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);

                //add task
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);

                //mark last task as completed
                browser.Last("a").Click();
                browser.Wait();

                browser.Last(".table tr").CheckClassAttribute(a => a.Contains("completed"),
                    "Last task is not marked as completed.");
            });
        }
    }
}