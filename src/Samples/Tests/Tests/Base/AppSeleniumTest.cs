﻿using System;
using System.Net;
using System.Runtime.CompilerServices;
using OpenQA.Selenium;
using Riganti.Selenium.AssertApi;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Base
{
    public class AppSeleniumTest : SeleniumTest
    {
        static AppSeleniumTest()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public AppSeleniumTest(ITestOutputHelper output) : base(output)
        {
        }

        public By SelectByDataUi(string selector)
            => SelectBy.CssSelector($"[data-ui='{selector}']");
        public By SelectByUiTestName(string selector)
                    => SelectBy.CssSelector($"[data-uitest-name='{selector}']");

        public virtual void RunInAllBrowsers(Action<IBrowserWrapper> testBody,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this, testBody, callerMemberName, callerFilePath, callerLineNumber);
        }
    }
}
