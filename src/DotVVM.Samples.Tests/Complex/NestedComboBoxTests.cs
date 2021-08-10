using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Complex
{
    public class NestedComboBoxTests : AppSeleniumTest
    {
        public NestedComboBoxTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Complex_NestedComboBox_HeavilyNested()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_NestedComboBox_HeavilyNested);
                browser.WaitUntilDotvvmInited();

                var selectedValue = browser.Single("selected-value", SelectByDataUi);
                AssertUI.TextEquals(selectedValue, "");

                var combobox = browser.Single("combobox", SelectByDataUi);
                combobox.Select(1);
                AssertUI.TextEquals(selectedValue, "2");

                combobox.Select(0);
                AssertUI.TextEquals(selectedValue, "");
            });
        }
    }
}
