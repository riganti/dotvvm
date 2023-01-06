using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class CsharpClientTests : AppSeleniumTest
    {
        [Fact]
        [Trait("Category", "aspnetcore-only")]
        public void Feature_CsharpClient_CSharpClient()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CsharpClient_CSharpClient);

                var value = browser.Single("value", SelectByDataUi);
                AssertUI.Value(value, "1");

                // check console access
                browser.Single("hello", SelectByDataUi).Click();
                AssertUI.TextEquals(browser.Single("console", SelectByDataUi), "Hello world");

                // read VM
                browser.Single("read-vm", SelectByDataUi).Click();
                AssertUI.TextEquals(browser.Single("read-vm-result", SelectByDataUi), "1");

                // patch VM
                browser.Single("patch-vm", SelectByDataUi).Click();
                AssertUI.TextEquals(value, "30");
                browser.Single("read-vm", SelectByDataUi).Click();
                AssertUI.TextEquals(browser.Single("read-vm-result", SelectByDataUi), "30");

                // call command
                browser.Single("named-command", SelectByDataUi).Click();
                AssertUI.TextEquals(value, "60");
            });
        }

        [Theory]
        [InlineData("MarshallByte", "0")]
        [InlineData("MarshallNullableByte", "")]
        [InlineData("MarshallSByte", "2")]
        [InlineData("MarshallNullableSByte", "4")]
        [InlineData("MarshallShort", "6")]
        [InlineData("MarshallNullableShort", "8")]
        [InlineData("MarshallUShort", "10")]
        [InlineData("MarshallNullableUShort", "12")]
        [InlineData("MarshallInt", "14")]
        [InlineData("MarshallNullableInt", "16")]
        [InlineData("MarshallUInt", "18")]
        [InlineData("MarshallNullableUInt", "20")]
        [InlineData("MarshallLong", "22")]
        [InlineData("MarshallNullableLong", "24")]
        [InlineData("MarshallULong", "26")]
        [InlineData("MarshallNullableULong", "28")]
        [InlineData("MarshallFloat", "2.46")]
        [InlineData("MarshallNullableFloat", "")]
        [InlineData("MarshallDouble", "9.1356")]
        [InlineData("MarshallNullableDouble", "19998")]
        [InlineData("MarshallDecimal", "2000000")]
        [InlineData("MarshallNullableDecimal", "2000002")]
        [InlineData("MarshallDateTime", "1/3/2020 3:04:05 AM")]
        [InlineData("MarshallNullableDateTime", "")]
        [InlineData("MarshallDateOnly", "Monday, October 12, 2020")]
        [InlineData("MarshallNullableDateOnly", "Friday, November 13, 2020")]
        [InlineData("MarshallTimeOnly", "7:00:05 AM")]
        [InlineData("MarshallNullableTimeOnly", "")]
        [InlineData("MarshallTimeSpan", "2.04:04:05")]
        [InlineData("MarshallNullableTimeSpan", "")]
        [InlineData("MarshallChar", "B")]
        [InlineData("MarshallNullableChar", "")]
        [InlineData("MarshallGuid", "c286c18d-ecd8-47e0-bfc6-6ce709c5d498")]
        [InlineData("MarshallNullableGuid", "")]
        [InlineData("MarshallString", "BABABA")]
        [InlineData("MarshallEnum", "Three")]
        [InlineData("MarshallNullableEnum", "")]
        [InlineData("MarshallObject", "2, ABC")]
        [InlineData("MarshallRecord", "3, DEF")]
        [InlineData("MarshallObjectArray", "3, ghi; 1, abc")]
        [InlineData("MarshallRecordArray", "4, jkl; 2, def")]
        [InlineData("MarshallIntArray", "5; 2")]
        [InlineData("MarshallNullableDoubleArray", "; 3")]
        [InlineData("MarshallStringArray", "def; abc")]
        [InlineData("MarshallTask", "8, pqr; 6, mno")]
        [Trait("Category", "aspnetcore-only")]
        public void Feature_CsharpClient_Marshalling(string section, string expected)
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CsharpClient_Marshalling);

                var p = browser.Single(section, SelectByDataUi);
                p.Single("input[type=button]").Click();
                AssertUI.TextEquals(p.Single("span"), expected);
            });
        }

        public CsharpClientTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
