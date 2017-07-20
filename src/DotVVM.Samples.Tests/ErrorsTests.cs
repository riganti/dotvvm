using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;
using System.Linq;

namespace DotVVM.Samples.Tests
{
    [TestClass]
    public class ErrorsTests : SeleniumTest
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
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException", StringComparison.OrdinalIgnoreCase) &&
                            s.Contains("Could not resolve type 'invalid'", StringComparison.OrdinalIgnoreCase)
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
                    .CheckIfInnerText(s => s.Contains("Could not implicitly convert expression"));

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

                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("Could not resolve identifier"));
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

                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("Could not resolve identifier") && s.Contains("NonExistingCommand"));
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

                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("Unexpected token"));
                browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("!"));
            });
        }

        [TestMethod]
        public void Error_EmptyBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_EmptyBinding);

                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("Identifier name was expected"));
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
                //browser.First("[class='errorUnderline']").CheckIfInnerText(s => s.Contains("DotVVM.Samples.BasicSamples.ViewModels.EmptyViewModel, DotVVM.Samples.Common"));
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
                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("is encrypted and cannot be used in JS"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("{{value: SomeProperty}}"));
            });
        }

        [TestMethod]
        public void Error_FieldInValueBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First(".exceptionMessage").CheckIfInnerText(s => s.Contains("Can not translate field"));
                browser.First(".errorUnderline").CheckIfInnerText(s => s.Contains("{{value: SomeField}}"));
            });
        }


        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside))]
        public void Error_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);
                browser.First("p.summary").CheckIfInnerText(s => s.Contains("control cannot have inner HTML"));
            });
        }

        [TestMethod]
        public void Error_MarkupControlInvalidViewModel()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MarkupControlInvalidViewModel);
                browser.First("p.summary")
                    .CheckIfInnerText(
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                            s.Contains("requires a DataContext of type")
                        );
            });
        }


        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_LabelClickableTest()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);

                //click Exception
                browser.First("label[for=menu_radio_exception]").Click();
                browser.First("#container_exception").CheckIfIsDisplayed();

                //click Cookies
                browser.First("label[for=menu_radio_cookies]").Click();
                browser.First("#container_cookies").CheckIfIsDisplayed();

                //click Request Headers
                browser.First("label[for=menu_radio_reqHeaders]").Click();
                browser.First("#container_reqHeaders").CheckIfIsDisplayed();

                //click DotVVM Markup
                browser.First("label[for=menu_radio_dothtml]").Click();
                browser.First("#container_dothtml").CheckIfIsDisplayed();
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_DotNetReferenceSourceRedirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First("label[for=menu_radio_exception]").Click();

                //find and click on github link
                var link = browser.FindElements("div.exceptionStackTrace span.docLinks a")
                           .First(s => s.Children.Any(c => c.GetTagName() == "img" && ((c.GetAttribute("src")?.IndexOf("referencesource.microsoft.com", StringComparison.OrdinalIgnoreCase) ?? -1) > -1)))
                           .GetAttribute("href");
                var startQuery = link.IndexOf("q=");
                var query = link.Substring(startQuery + 2);
                Log("query: " + query);
                var specificLink = "http://referencesource.microsoft.com/api/symbols/?symbol=" + query;
                using (var wc = new System.Net.WebClient())
                {
                    var downloadedString = wc.DownloadString(specificLink);
                    if (downloadedString.IndexOf("No results found", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        throw new Exception("The relevant docs page on referencesource.microsoft.com was not found.");
                    }
                }

            });
        }

        [TestMethod, Ignore]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_GitHubRedirect()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                //open exception tab
                browser.First("label[for=menu_radio_exception]").Click();

                //find and click on github link
                var link = browser.FindElements("div.exceptionStackTrace  span.docLinks  a")
                    .First(s => s.Children.Any(c => c.GetTagName() == "img" && ((c.GetAttribute("src")?.IndexOf("github", StringComparison.OrdinalIgnoreCase) ?? -1) > -1)))
                    .GetAttribute("href");
                var wr = (System.Net.HttpWebRequest)System.Net.WebRequest.CreateHttp(link);

                using (var response = wr.GetResponse())
                {
                    if ((response as System.Net.HttpWebResponse).StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new Exception($"GitHub link does not exist. Link: {link}");
                    }
                }
            });
        }

    }
}