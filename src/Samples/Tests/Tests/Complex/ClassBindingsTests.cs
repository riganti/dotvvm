using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class ClassBindingsTests : AppSeleniumTest
    {
        [Fact]
        public void Complex_ClassBindings_AttributeAndPropertyGroup()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_ClassBindings_ClassBindings);
                browser.WaitUntilDotvvmInited();

                var target = browser.Single("target", SelectByDataUi);
                var textBox = browser.Single("classes", SelectByDataUi);
                textBox.SendKeys("orange");
                textBox.SendKeys(Keys.Tab);
                AssertUI.HasClass(target, "orange");

                browser.Single("inverted", SelectByDataUi).Click();
                browser.Single("border", SelectByDataUi).Click();
                AssertUI.HasClass(target, "orange");
                AssertUI.HasClass(target, "inverted");
                AssertUI.HasClass(target, "border");
            });
        }

        public ClassBindingsTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
