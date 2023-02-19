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
using Xunit.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class CustomPrimitiveTypesTests : AppSeleniumTest
    {
        public CustomPrimitiveTypesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("/96c37b99-5fd5-448c-8a64-977ae11b8b8b?Id=c2654a1f-3781-49a8-911b-c7346db166e0", "96c37b99-5fd5-448c-8a64-977ae11b8b8b", "c2654a1f-3781-49a8-911b-c7346db166e0")]
        public void Feature_CustomPrimitiveTypes_Basic(string urlSuffix, string expectedRouteParam, string expectedQueryParam)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_CustomPrimitiveTypes_Basic + urlSuffix);

                var selectedItem = browser.Single("selected-item", SelectByDataUi);
                var selectedItemCombo = browser.Single("selected-item-combo", SelectByDataUi);
                var selectedItemValidator = browser.Single("selected-item-validator", SelectByDataUi);
                var selectedItemNullable = browser.Single("selected-item-nullable", SelectByDataUi);
                var selectedItemNullableCombo = browser.Single("selected-item-nullable-combo", SelectByDataUi);
                var selectedItemNullableValidator = browser.Single("selected-item-nullable-validator", SelectByDataUi);
                var idFromRoute = browser.Single("id-from-route", SelectByDataUi);
                var idFromQuery = browser.Single("id-from-query", SelectByDataUi);
                var routeLink = browser.Single("routelink", SelectByDataUi);
                var command = browser.Single("command", SelectByDataUi);
                var staticCommand = browser.Single("static-command", SelectByDataUi);
                var staticCommandResult = browser.Single("static-command-result", SelectByDataUi);
                var binding = browser.Single("binding", SelectByDataUi);
                
                // check route link
                AssertUI.TextEquals(idFromRoute, expectedRouteParam);
                AssertUI.TextEquals(idFromQuery, expectedQueryParam);
                AssertUI.Attribute(routeLink, "href", v => v.Contains(urlSuffix));

                // select in first list
                AssertUI.TextEquals(binding, "My id values are and");
                AssertUI.TextEquals(selectedItem, "");
                selectedItemCombo.Select(0);
                AssertUI.TextEquals(selectedItem, "96c37b99-5fd5-448c-8a64-977ae11b8b8b");
                selectedItemCombo.Select(1);
                AssertUI.TextEquals(selectedItem, "c2654a1f-3781-49a8-911b-c7346db166e0");
                AssertUI.TextEquals(binding, "My id values are c2654a1f-3781-49a8-911b-c7346db166e0 and");

                // select in second list
                AssertUI.TextEquals(selectedItemNullable, "");
                selectedItemNullableCombo.Select(3);
                AssertUI.TextEquals(selectedItemNullable, "e467a201-9ab7-4cd5-adbf-66edd03f6ae1");
                AssertUI.TextEquals(binding, "My id values are c2654a1f-3781-49a8-911b-c7346db166e0 and E467A201-9AB7-4CD5-ADBF-66EDD03F6AE1");
                selectedItemNullableCombo.Select(0);
                AssertUI.TextEquals(selectedItemNullable, "");

                // command and validation
                AssertUI.IsNotDisplayed(selectedItemValidator);
                AssertUI.IsNotDisplayed(selectedItemNullableValidator);
                command.Click();

                AssertUI.IsNotDisplayed(selectedItemValidator);
                AssertUI.IsDisplayed(selectedItemNullableValidator);
                AssertUI.TextEquals(selectedItemNullableValidator, "The SelectedItemNullableId field is required.");
                selectedItemCombo.Select(0);
                selectedItemNullableCombo.Select(1);
                command.Click();

                AssertUI.IsDisplayed(selectedItemValidator);
                AssertUI.IsNotDisplayed(selectedItemNullableValidator);
                AssertUI.TextEquals(selectedItemValidator, "Valid property path Invalid property path");
                selectedItemCombo.Select(1);
                command.Click();

                AssertUI.IsNotDisplayed(selectedItemValidator);
                AssertUI.IsNotDisplayed(selectedItemNullableValidator);

                // static command
                staticCommand.Click();
                AssertUI.TextEquals(staticCommandResult, "54162c7e-cdcc-4585-aa92-2e78be3f0c75");
            });
        }
    }
}
