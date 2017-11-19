using System;
using System.Runtime.CompilerServices;
using OpenQA.Selenium;
using Riganti.Selenium.AssertApi;
using Riganti.Selenium.Core;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New
{
    public class AppSeleniumTest : SeleniumTest
    {
        public AppSeleniumTest(ITestOutputHelper output) : base(output)
        {
        }
        public By SelectByDataUi(string selector)
            => SelectBy.CssSelector($"[data-ui='{selector}']");


        public virtual void RunInAllBrowsers(Action<BrowserWrapperAssertApi> testBody,
            [CallerMemberName] string callerMemberName = "", 
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, testBody, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}