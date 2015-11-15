using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dotvvm.Samples.Tests;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class TextBoxTests : SeleniumTestBase
    {
        [TestMethod]
        public void Control_TextBox()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl( SamplesRouteUrls.ControlSamples_TextBox_TextBox);
                
                browser.First("#TextBox1").CheckTagName("input");
                browser.First("#TextBox2").CheckTagName("input");
                browser.First("#TextArea1").CheckTagName("textarea");
                browser.First("#TextArea2").CheckTagName("textarea");
            });
        }
    }
}