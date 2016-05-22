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
                browser.ElementAt(".errorUnderline", 1).CheckIfInnerText(s => s.Contains("{{value: }}"));
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
    }
}