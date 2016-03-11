using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                browser.NavigateToUrl("Errors/MissingViewModel");
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
                browser.NavigateToUrl("Errors/InvalidViewModel");
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                            s.Contains("required in the @viewModel directive in was not found!")
                            );
            });
        }

        [TestMethod]
        public void Error_NonExistingControl()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/NonExistingControl");
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
                browser.NavigateToUrl("Errors/NonExistingProperty");
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
                browser.NavigateToUrl("Errors/NotAllowedHardCodedPropertyValue");
                browser.First("[class='exceptionMessage']")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("cannot contain hard coded value.")
                            );

                browser.First("[class='errorUnderline']")
                    .CheckIfInnerText(s => s.Contains("NotAllowedHardCodedValue"));
            });
        }

        [TestMethod]
        public void Error_WrongPropertyValue()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/WrongPropertyValue");
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
                browser.NavigateToUrl("Errors/MissingRequiredProperty");
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("must be set"));
                browser.First("[class='source-errorLine']").CheckIfInnerText(s => s.Contains("dot:CheckBox"));
            });
        }

        [TestMethod]
        public void Error_MissingRequiredProperty2()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MissingRequiredProperty2");

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is missing required properties"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("dot:GridViewTextColumn "));
            });
        }

        [TestMethod]
        public void Error_BindingInvalidProperty()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/BindingInvalidProperty");

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
                browser.NavigateToUrl("Errors/BindingInvalidCommand");

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
                browser.NavigateToUrl("Errors/MalformedBinding");

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("!"));
            });
        }


        [TestMethod]
        public void Error_EmptyBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/EmptyBinding");

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.ElementAt(".errorUnderline", 1).CheckIfInnerText(s => s.Contains("{{value: }}"));
            });
        }

        [TestMethod]
        public void Error_MasterPageRequiresDifferentViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MasterPageRequiresDifferentViewModel");

                //TODO:  !!! In error page, viewModel directive should by underlined !!!
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Master page requires viewModel"));
                //browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("DotVVM.Samples.BasicSamples.ViewModels.EmptyViewModel, DotVVM.Samples.BasicSamples"));
            });
        }
    }
}