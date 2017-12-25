
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class StaticCommandTests : AppSeleniumTest
    {
        public StaticCommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_StaticCommand_ValueAssignmentControl()
        {

            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_StaticCommand_StaticCommand_ValueAssignmentInControl);

                var intResult = browser.First("#intResult");
                var vmResult = browser.First("#vmResult");
                var stringResult = browser.First("#stringResult");
                var boolResult = browser.First("#boolResult");


                var setTrue = browser.First("#setTrue");
                var setFalse = browser.First("#setFalse");

                var setNull = browser.First("#setNull");
                var setStringValue = browser.First("#setStringValue");

                var setZero = browser.First("#setZero");
                var setTen = browser.First("#setTen");

                var vmSetTrue = browser.First("#vmSetTrue");
                var vmSetFalse = browser.First("#vmSetFalse");


                AssertUI.TextEquals(intResult, "0");
                AssertUI.TextEquals(stringResult, "");
                AssertUI.TextEquals(vmResult, "false");
                AssertUI.TextEquals(boolResult, "false");

                //bool
                setTrue.Click();
                AssertUI.TextEquals(boolResult, "true");
                setFalse.Click();
                AssertUI.TextEquals(boolResult, "false");

                //int
                setTen.Click();
                AssertUI.TextEquals(intResult, "10");
                setZero.Click();
                AssertUI.TextEquals(intResult, "0");

                //string
                setStringValue.Click();
                AssertUI.TextEquals(stringResult, "value");
                setNull.Click();
                AssertUI.TextEquals(stringResult, "");

                //vm value
                vmSetTrue.Click();
                AssertUI.TextEquals(vmResult, "true");
                vmSetFalse.Click();
                AssertUI.TextEquals(vmResult, "false");
            });
        }
    }
}
