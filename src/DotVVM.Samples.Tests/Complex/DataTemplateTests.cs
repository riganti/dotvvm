using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class DataTemplateTests : SeleniumTestBase
    {
        [TestMethod]
        public void Complex_EmptyDataTemplateRepeaterGridView()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_DataTemplate_EmptyDataTemplateRepeaterGridView);
                browser.Wait();
                Action<string> isDisplayed = id =>  browser.CheckIfIsDisplayed("#" + id);
                Action<string> isHidden = id => browser.CheckIfIsNotDisplayed("#"+ id);
                Action<string> isNotPresent = id => browser.FindElements("#" + id).ThrowIfDifferentCountThan(0);

                isHidden("marker1_parent");
                isDisplayed("marker1");

                isNotPresent("marker2_parent");
                isDisplayed("marker2");

                isHidden("marker3_parent");
                isDisplayed("marker3");

                isNotPresent("marker4_parent");
                isDisplayed("marker4");

                isDisplayed("nonempty_marker1_parent");
                isHidden("nonempty_marker1");

                isDisplayed("nonempty_marker2_parent");
                isNotPresent("nonempty_marker2");

                isDisplayed("nonempty_marker3_parent");
                isHidden("nonempty_marker3");

                isDisplayed("nonempty_marker4_parent");
                isNotPresent("nonempty_marker4");

                isHidden("null_marker1_parent");
                isDisplayed("null_marker1");

                isNotPresent("null_marker2_parent");
                isDisplayed("null_marker2");

                isHidden("null_marker3_parent");
                isDisplayed("null_marker3");

                isNotPresent("null_marker4_parent");
                isDisplayed("null_marker4");
            });
        }
    }
}
