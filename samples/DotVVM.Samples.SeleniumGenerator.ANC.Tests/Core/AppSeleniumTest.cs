
using System;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects;
using OpenQA.Selenium;
using Riganti.Selenium.AssertApi;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Drivers;
using Xunit.Sdk;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests
{
    public abstract class AppSeleniumTest : SeleniumTest
    {
        protected AppSeleniumTest() : base(new TestOutputHelper())
        {
        }
        protected void RunInAllBrowsers<T>(Action<IBrowserWrapper, T> action, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) where T : SeleniumHelperBase
        {
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, browser => {
                var internalBrowser = browser._GetInternalWebDriver();
                var pageObject = Activator.CreateInstance(typeof(T), internalBrowser, null, null);
                browser.NavigateToUrl();
                action(browser, (T)pageObject);
            }, callerMemberName, callerFilePath, callerLineNumber);

        }
    }
    public static class Extensions
    {
        public static T InitRootPageObject<T>(this IBrowserWrapper wrapper) where T : SeleniumHelperBase
        {
            var internalBrowser = wrapper._GetInternalWebDriver();
            var pageObject = (T)Activator.CreateInstance(typeof(T), internalBrowser, null, null);
            return pageObject;
        }
    }
}

