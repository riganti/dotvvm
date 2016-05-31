using System;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests
{
    [TestClass]
    public class ErrorsTests : SeleniumTestBase
    {
        [TestMethod]
        public void Error_MissingViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingViewModel);
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                            s.Contains("@viewModel") &&
                            s.Contains("missing")
                        );
            });
        }

        [TestMethod]
        public void Error_InvalidViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_InvalidViewModel);
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                            s.Contains("required in the @viewModel directive was not found!")
                            );
            });
        }

        [TestMethod]
        public void Error_NonExistingControl()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NonExistingControl);
                browser.First("[class=exceptionMessage]")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("dot:NonExistingControl") &&
                            s.Contains("could not be resolved")
                            );
                browser.First("[class='errorUnderline']")
                    .CheckIfInnerTextEquals("<dot:NonExistingControl />", false);
            });
        }

        [TestMethod]
        public void Error_NonExistingProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NonExistingProperty);
                browser.First("[class='exceptionMessage']")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("does not have a property 'NonExistingProperty'")
                            );

                browser.First("[class='errorUnderline']")
                    .CheckIfInnerText(s => s.Contains("NonExistingProperty"));
            });
        }

        [TestMethod]
        public void Error_NotAllowedHardCodedPropertyValue()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NotAllowedHardCodedPropertyValue);
                browser.First("[class='exceptionMessage']")
                .CheckIfInnerText(
                        s =>
                            s.ToLower().Contains("String was not recognized as a valid Boolean.".ToLower())
                            , "Expected message is 'String was not recognized as a valid Boolean.'");

                browser.First("[class='errorUnderline']")
                    .CheckIfInnerText(s => s.Contains("NotAllowedHardCodedValue"));
            });
        }

        [TestMethod]
        public void Error_WrongPropertyValue()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_WrongPropertyValue);
                browser.First("[class='exceptionMessage']")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("could not implicitly convert expression")
                            );

                browser.First("[class='errorUnderline']")
                    .CheckIfInnerText(s => s.Contains("NotAllowedValue"));
            });
        }

        [TestMethod]
        public void Error_MissingRequiredProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingRequiredProperty);
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("must be set"));
                browser.First("[class='source-errorLine']").CheckIfInnerText(s => s.Contains("dot:CheckBox"));
            });
        }

        [TestMethod]
        public void Error_MissingRequiredProperty2()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingRequiredProperty2);

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is missing required properties"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("dot:GridViewTextColumn "));
            });
        }

        [TestMethod]
        public void Error_BindingInvalidProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_BindingInvalidProperty);

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid"));
                browser.First("[class='source-errorLine']").CheckIfInnerText(s => s.Contains("InvalidPropertyName"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("InvalidPropertyName"));
            });
        }

        [TestMethod]
        public void Error_BindingInvalidCommand()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_BindingInvalidCommand);

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.First("[class='source-errorLine']").CheckIfInnerText(s => s.Contains("NonExistingCommand"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("NonExistingCommand"));
            });
        }

        [TestMethod]
        public void Error_MalformedBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MalformedBinding);

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("!"));
            });
        }

        [TestMethod]
        public void Error_EmptyBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_EmptyBinding);

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("{{value: }}"));

            });
        }

        [TestMethod]
        public void Error_MasterPageRequiresDifferentViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MasterPageRequiresDifferentViewModel);

                //TODO:  !!! In error page, viewModel directive should by underlined !!!
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Master page requires viewModel"));
                //browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("DotVVM.Samples.BasicSamples.ViewModels.EmptyViewModel, DotVVM.Samples.BasicSamples"));
            });
        }

        [TestMethod]
        public void Error_ControlUsageValidation()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_ControlUsageValidation);
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Text property and inner content") && s.Contains("cannot be set at the same time"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("Click=\"{command: 5}\" Text=\"Text property\""));
            });
        }

        [TestMethod]
        public void Error_EncryptedPropertyInValueBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_EncryptedPropertyInValueBinding);
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Could not compile binding to Javascript"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("{{value: SomeProperty}}"));
            });
        }

        [TestMethod]
        public void Error_FieldInValueBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Could not compile binding to Javascript"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("{{value: SomeField}}"));
            });
        }

        [TestMethod]
        public void LabelClickableTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);

                //click Exception
                browser.First("body > div > label:nth-child(4)").Click();
                browser.First("#container_exception > div:nth-child(1) > span.exceptionMessage")
                    .CheckIfInnerText(s => s.Contains("cannot be translated to knockout"));

                //click Cookies
                browser.First("body > div > label:nth-child(6)").Click();
                browser.First("#container_cookies > table > tbody > tr:nth-child(1) > th:nth-child(1)")
                   .CheckIfInnerText(s => s.Contains("Variable"));

                //click Request Headers
                browser.First("body > div > label:nth-child(8)").Click();
                browser.First("#container_reqHeaders > table > tbody > tr:nth-child(1) > th:nth-child(1)")
                    .CheckIfInnerText(s => s.Contains("Variable"));

                //click Environment
                browser.First("body > div > label:nth-child(10)").Click();
                browser.First("#container_env > table > tbody > tr:nth-child(1) > th:nth-child(1)")
                   .CheckIfInnerText(s => s.Contains("Variable"));

                //click DotVVM Markup
                browser.First("body > div > label:nth-child(2)").Click();
                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("cannot be translated to knockout"));
            });
        }

        [TestMethod]
        public void Exception_GitHubRedirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First("body > div > label:nth-child(4)").Click();
                browser.First(
                    "#container_exception > div:nth-child(1) > div.exceptionStackTrace > div:nth-child(1) > span.docLinks > a > img")
                    .Click();
                browser.SwitchToTab(1);
                browser.Wait(3000);
                browser.CompareUrl(
                    "https://github.com/riganti/dotvvm/blob/master/src/DotVVM.Framework/Compilation/Javascript/JavascriptTranslator.cs#L404");

                browser.First(
                    "#container_exception > div:nth-child(2) > div.exceptionStackTrace > div:nth-child(35) > span.docLinks > a > img")
                    .Click();
            });
        }

        [TestMethod]
        public void Exception_dotNetRedirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First("body > div > label:nth-child(4)").Click();
                browser.First(
                    "#container_exception > div:nth-child(2) > div.exceptionStackTrace > div:nth-child(35) > span.docLinks > a > img")
                    .Click();
                browser.SwitchToTab(1);
                browser.Wait(3000);
                browser.CompareUrl(
                    "http://referencesource.microsoft.com/#q=System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess");

                
            });
        }
    }
}