using System;
using System.Runtime.CompilerServices;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests
{
    public class AppSeleniumTest : SeleniumTest
    {
        public void RunInAllBrowsers(Action<IBrowserWrapperFluentApi> testBody, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            FluentApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, testBody, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}
