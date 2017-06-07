using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class TaskListTests : SeleniumTest
    {
        [TestMethod]
        public void Complex_TaskListAsyncCommands()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_TaskList_TaskListAsyncCommands);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);

                //add task
                browser.SendKeys("input[type=text]", "DotVVM");
                browser.ElementAt("input[type=button]",0).Click();
                browser.Wait(500);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);

                //mark last task as completed
                browser.Last("a").Click();
                browser.Wait(500);

                browser.Last(".table tr").CheckClassAttribute(a => a.Contains("completed"), "Last task is not marked as completed.");

                browser.ElementAt("input[type=button]", 1).Click().Wait(1000);
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(5);
            });
        }

        [TestMethod]
        public void Complex_ServerRenderedTaskList()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_TaskList_ServerRenderedTaskList);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);

                //add task
                browser.SendKeys("input[type=text]", "DotVVM");
                browser.Click("input[type=button]");
                browser.Wait(500);

                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);

                //mark last task as completed
                browser.Last("a").Click();
                browser.Wait(500);

                browser.Last(".table tr").CheckClassAttribute(a => a.Contains("completed"),
                    "Last task is not marked as completed.");
            });
        }
    }
}