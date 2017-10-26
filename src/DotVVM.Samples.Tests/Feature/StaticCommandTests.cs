
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class StaticCommandTests : AppSeleniumTest
    {
        [TestMethod]
        public void Feature_StaticCommand_StaticCommand()
        {
            RunInAllBrowsers(browser =>
                {
                    foreach(var b in browser.FindElements("input[type=button]"))
                    {
                        browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand);
                        browser.Wait();
                        b.Click();
                        browser.First("span").CheckIfInnerTextEquals("Hello Deep Thought!");
                    }
                });
        }

        [TestMethod]
        public void Feature_StaticCommand_StaticCommand_ComboBoxSelectionChanged()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ComboBoxSelectionChanged);
                Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(browser);
            });
        }

        [TestMethod]
        public void Feature_StaticCommand_StaticCommand_ComboBoxSelectionChanged_Objects()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ComboBoxSelectionChanged_Objects);
                Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(browser);
            });
        }

        private static void Feature_StaticCommand_ComboBoxSelectionChangedViewModel_Core(BrowserWrapper browser)
        {
            browser.Wait();

            // select second value in the first combo box, the second one should select the second value too 
            browser.ElementAt("select", 0).Select(1).Wait();
            browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 1).CheckIfIsSelected();

            // select third value in the first combo box, the second one should select the third value too 
            browser.ElementAt("select", 0).Select(2).Wait();
            browser.ElementAt("select", 0).ElementAt("option", 2).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 2).CheckIfIsSelected();

            // select first value in the first combo box, the second one should select the first value too 
            browser.ElementAt("select", 0).Select(0).Wait();
            browser.ElementAt("select", 0).ElementAt("option", 0).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 0).CheckIfIsSelected();

            // click the first button - the second value should be selected in the first select, the second select should not change
            browser.ElementAt("input", 0).Click().Wait();
            browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 0).CheckIfIsSelected();

            // click the second button - the third value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 1).Click().Wait();
            browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 2).CheckIfIsSelected();

            // click the third button - the first value should be selected in the second select, the first select should not change
            browser.ElementAt("input", 2).Click().Wait();
            browser.ElementAt("select", 0).ElementAt("option", 1).CheckIfIsSelected();
            browser.ElementAt("select", 1).ElementAt("option", 0).CheckIfIsSelected();
        }
    }
}
