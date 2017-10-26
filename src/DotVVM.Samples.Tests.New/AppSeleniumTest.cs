using System;
using System.Runtime.CompilerServices;
using Riganti.Utils.Testing.Selenium.AssertApi;
using Riganti.Utils.Testing.Selenium.Core;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New
{
    public class AppSeleniumTest : SeleniumTest
    {
        public AppSeleniumTest(ITestOutputHelper output) : base(output)
        {
        }

        public virtual void RunInAllBrowsers(Action<BrowserWrapperAssertApi> testBody,
            [CallerMemberName] string callerMemberName = "", 
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, testBody, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}