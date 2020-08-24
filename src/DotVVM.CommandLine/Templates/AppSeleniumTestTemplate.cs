namespace DotVVM.CommandLine.Templates
{
    public static class AppSeleniumTestTemplate
    {
        public static string TransformText(string @namespace)
        {
            return
$@"using System;
using System.Runtime.CompilerServices;
using Riganti.Selenium.AssertApi;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit.Sdk;

namespace {@namespace}
{{
    public abstract class AppSeleniumTest : SeleniumTest
    {{
        protected AppSeleniumTest() : base(new TestOutputHelper())
        {{
        }}

        protected void RunInAllBrowsers<T>(
            Action<IBrowserWrapper, T> action,
            [CallerMemberName] string callerMemberName = """",
            [CallerFilePath] string callerFilePath = """",
            [CallerLineNumber] int callerLineNumber = 0)
            where T : SeleniumHelperBase
        {{
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, browser =>
            {{
                var internalBrowser = browser._GetInternalWebDriver();
                var pageObject = Activator.CreateInstance(typeof(T), internalBrowser, null, null);
                browser.NavigateToUrl();
                action(browser, (T)pageObject);
            }},
            callerMemberName,
            callerFilePath,
            callerLineNumber);
        }}
    }}

    public static class Extensions
    {{
        public static T InitRootPageObject<T>(this IBrowserWrapper wrapper) where T : SeleniumHelperBase
        {{
            var internalBrowser = wrapper._GetInternalWebDriver();
            var pageObject = (T)Activator.CreateInstance(typeof(T), internalBrowser, null, null);
            return pageObject;
        }}
    }}
}}
";
        }
    }
}
