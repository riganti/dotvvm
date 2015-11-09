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
        public void MissingViewModelTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MissingViewModel");
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Exceptions.DotvvmCompilationException") &&
                            s.Contains("@viewModel") &&
                            s.Contains("missing")
                        );
            });
        }

        [TestMethod]
        public void InvalidViewModelTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/InvalidViewModel");
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Exceptions.DotvvmCompilationException") &&
                            s.Contains("required in the @viewModel directive in was not found!")
                            );
            });
        }

        [TestMethod]
        public void NonExistingControlTest()
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
        public void NonExistingPropertyTest()
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
        public void NotAllowedHardCodedPropertyValueTest()
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
        public void WrongPropertyValueTest()
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
        public void MissingRequiredPropertyTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MissingRequiredProperty");
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("must be set"));
                browser.First("[class='source-errorLine']").CheckIfInnerText(s => s.Contains("dot:CheckBox"));
            });
        }

        [TestMethod]
        public void MissingRequiredProperty2Test()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MissingRequiredProperty2");

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is missing required properties"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("dot:GridViewTextColumn "));
            });
        }

        [TestMethod]
        public void BindingInvalidPropertyTest()
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
        public void BindingInvalidCommandTest()
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
        public void MalformedBindingTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MalformedBinding");

                browser.First("p.summary").CheckIfInnerText(s => s.Contains("is not valid") && s.Contains("The binding"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("command") && s.Contains("something"));
            });
        }

        [TestMethod]
        public void MasterPageRequeiresDifferentViewModelTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl("Errors/MalformedBinding");

                //TODO:  !!! In error page, viewModel directive should by underlined !!!
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("Master page requires viewModel"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("DotVVM.Samples.BasicSamples.ViewModels.EmpltyViewModel, DotVVM.Samples.BasicSamples"));
            });
        }
    }
}