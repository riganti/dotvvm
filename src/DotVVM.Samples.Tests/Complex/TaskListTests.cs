using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class TaskListTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_TaskList()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_TaskList_TaskList);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);

                //add task
                browser.SendKeys("input[type=text]", "DotVVM");
                browser.ElementAt("input[type=button]",0).Click();
                browser.Wait();

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);

                //mark last task as completed
                browser.Last("a").Click();
                browser.Wait();

                browser.Last(".table tr").CheckClassAttribute(a => a.Contains("completed"),
                    "Last task is not marked as completed.");

                browser.ElementAt("input[type=button]", 1).Click();
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(5);
            });
        }
    }
}