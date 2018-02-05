﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests
{
    public class ErrorsTests : AppSeleniumTest
    {
        [Fact]
        public void Error_MissingViewModel()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingViewModel);
                AssertUI.InnerText(browser.First("p.summary")
                      ,
                          s =>
                              s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                              s.Contains("@viewModel") &&
                              s.Contains("missing")
                          );
            });
        }

        [Fact]
        public void Error_InvalidViewModel()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_InvalidViewModel);
                AssertUI.InnerText(browser.First("p.summary")
                    ,
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException", StringComparison.OrdinalIgnoreCase) &&
                            s.Contains("Could not resolve type 'invalid'", StringComparison.OrdinalIgnoreCase)
                            );
            });
        }

        [Fact]
        public void Error_NonExistingControl()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NonExistingControl);
                AssertUI.InnerText(browser.First("[class=exceptionMessage]")
                    ,
                        s =>
                            s.Contains("dot:NonExistingControl") &&
                            s.Contains("could not be resolved")
                            );
                AssertUI.InnerTextEquals(browser.First("[class='errorUnderline']")
                    , "<dot:NonExistingControl />", false);
            });
        }

        [Fact]
        public void Error_NonExistingProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NonExistingProperty);
                AssertUI.InnerText(browser.First("[class='exceptionMessage']")
                    ,
                        s =>
                            s.Contains("does not have a property 'NonExistingProperty'")
                            );

                AssertUI.InnerText(browser.First("[class='errorUnderline']")
                    , s => s.Contains("NonExistingProperty"));
            });
        }

        [Fact]
        public void Error_NotAllowedHardCodedPropertyValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_NotAllowedHardCodedPropertyValue);
                AssertUI.InnerText(browser.First("[class='exceptionMessage']")
                ,
                        s =>
                            s.ToLower().Contains("String was not recognized as a valid Boolean.".ToLower())
                            , "Expected message is 'String was not recognized as a valid Boolean.'");

                AssertUI.InnerText(browser.First("[class='errorUnderline']")
                    , s => s.Contains("NotAllowedHardCodedValue"));
            });
        }

        [Fact]
        public void Error_WrongPropertyValue()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_WrongPropertyValue);
                AssertUI.InnerText(browser.First("[class='exceptionMessage']")
                    , s => s.Contains("Could not implicitly convert expression"));

                AssertUI.InnerText(browser.First("[class='errorUnderline']")
                    , s => s.Contains("NotAllowedValue"));
            });
        }

        [Fact]
        public void Error_MissingRequiredProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingRequiredProperty);
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("must be set"));
                AssertUI.InnerText(browser.First("[class='source-errorLine']"), s => s.Contains("dot:CheckBox"));
            });
        }

        [Fact]
        public void Error_MissingRequiredProperty2()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MissingRequiredProperty2);

                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("is missing required properties"));
                AssertUI.InnerText(browser.First("[class='errorUnderline']"), s => s.Contains("dot:GridViewTextColumn "));
            });
        }

        [Fact]
        public void Error_BindingInvalidProperty()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_BindingInvalidProperty);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Could not resolve identifier"));
                AssertUI.InnerText(browser.First("[class='source-errorLine']"), s => s.Contains("InvalidPropertyName"));
                AssertUI.InnerText(browser.First("[class='errorUnderline']"), s => s.Contains("InvalidPropertyName"));
            });
        }

        [Fact]
        public void Error_BindingInvalidCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_BindingInvalidCommand);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Could not resolve identifier") && s.Contains("NonExistingCommand"));
                AssertUI.InnerText(browser.First("[class='source-errorLine']"), s => s.Contains("NonExistingCommand"));
                AssertUI.InnerText(browser.First("[class='errorUnderline']"), s => s.Contains("NonExistingCommand"));
            });
        }

        [Fact]
        public void Error_MalformedBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MalformedBinding);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Unexpected token"));
                AssertUI.InnerText(browser.First("[class='errorUnderline']"), s => s.Contains("!"));
            });
        }

        [Fact]
        public void Error_EmptyBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_EmptyBinding);

                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Identifier name was expected"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("{{value: }}"));
            });
        }

        [Fact]
        public void Error_MasterPageRequiresDifferentViewModel()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MasterPageRequiresDifferentViewModel);

                //TODO:  !!! In error page, viewModel directive should by underlined !!!
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("Master page requires viewModel"));
                //browser.First("[class='errorUnderline']"),s => s.Contains("DotVVM.Samples.BasicSamples.ViewModels.EmptyViewModel, DotVVM.Samples.Common"));
            });
        }

        [Fact]
        public void Error_ControlUsageValidation()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_ControlUsageValidation);
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("Text property and inner content") && s.Contains("cannot be set at the same time"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("Click=\"{command: 5}\" Text=\"Text property\""));
            });
        }

        [Fact]
        public void Error_EncryptedPropertyInValueBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_EncryptedPropertyInValueBinding);
                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("is encrypted and cannot be used in JS"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("{{value: SomeProperty}}"));
            });
        }

        [Fact]
        public void Error_FieldInValueBinding()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                AssertUI.InnerText(browser.First(".exceptionMessage"), s => s.Contains("Can not translate field"));
                AssertUI.InnerText(browser.First(".errorUnderline"), s => s.Contains("{{value: SomeField}}"));
            });
        }


        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside))]
        public void Error_InputTypeButton_HtmlContentInside()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Button_InputTypeButton_HtmlContentInside);
                AssertUI.InnerText(browser.First("p.summary"), s => s.Contains("control cannot have inner HTML"));
            });
        }

        [Fact]
        public void Error_MarkupControlInvalidViewModel()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_MarkupControlInvalidViewModel);
                AssertUI.InnerText(browser.First("p.summary")
                    ,
                        s =>
                            s.Contains("DotVVM.Framework.Compilation.DotvvmCompilationException") &&
                            s.Contains("requires a DataContext of type")
                        );
            });
        }


        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_LabelClickableTest()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);

                //click Exception
                browser.First("label[for=menu_radio_stack_trace]").Click();
                AssertUI.IsDisplayed(browser.First("#container_stack_trace"));

                //click Cookies
                browser.First("label[for=menu_radio_cookies]").Click();
                AssertUI.IsDisplayed(browser.First("#container_cookies"));

                //click Request Headers
                browser.First("label[for=menu_radio_reqHeaders]").Click();
                AssertUI.IsDisplayed(browser.First("#container_reqHeaders"));

                //click DotVVM Markup
                browser.First("label[for=menu_radio_dothtml]").Click();
                AssertUI.IsDisplayed(browser.First("#container_dothtml"));
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_DotNetReferenceSourceRedirect()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.Errors_FieldInValueBinding);
                browser.First("label[for=menu_radio_stack_trace]").Click();

                //find and click on github link
                var link = browser.FindElements("div.exceptionStackTrace span.docLinks a")
                           .First(s => s.Children.Any(c => c.GetTagName() == "img" && ((c.GetAttribute("src")?.IndexOf("referencesource.microsoft.com", StringComparison.OrdinalIgnoreCase) ?? -1) > -1)))
                           .GetAttribute("href");
                var startQuery = link.IndexOf("q=");
                var query = link.Substring(startQuery + 2);
                //Log("query: " + query);
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

        [Fact(Skip = "Test is not reliable.")]
        [SampleReference(nameof(SamplesRouteUrls.Errors_FieldInValueBinding))]
        public void Error_ExceptionWindow_GitHubRedirect()
        {
            RunInAllBrowsers(browser => {
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

        public ErrorsTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
