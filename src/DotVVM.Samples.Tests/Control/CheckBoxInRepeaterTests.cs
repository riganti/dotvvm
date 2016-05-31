using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class CheckBoxInRepeaterTests:SeleniumTestBase
    {
        [TestMethod]
        public void CheckBoxInRepeaterTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_CheckBox_CheckboxInRepeater);
            });
        }
    }
}
