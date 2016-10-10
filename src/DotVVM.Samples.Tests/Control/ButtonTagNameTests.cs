using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class ButtonTagNameTests : SeleniumTestBase
    {

        [TestMethod]
        public void Control_ButtonTagName()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_ButtonTagName);
                
                browser.First("#ButtonTextProperty").CheckTagName("button");
                browser.First("#ButtonTextBinding").CheckTagName("button");
                browser.First("#InputTextProperty").CheckTagName("input");
                browser.First("#InputTextBinding").CheckTagName("input");
                browser.First("#ButtonInnerText").CheckTagName("button");

                browser.First("#ButtonTextPropertyUpperCase").CheckTagName("button");
                browser.First("#ButtonTextBindingUpperCase").CheckTagName("button");
                browser.First("#InputTextPropertyUpperCase").CheckTagName("input");
                browser.First("#ButtonInnerTextUpperCase").CheckTagName("button");
            });
        }

    }
}