using System.Net;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Feature
{
    public class ReturnedFileTests : AppSeleniumTest
    {
        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Simple()
        {
            RunInAllBrowsers(browser => {
                ReturnedFileDownload(browser, "Hello DotVVM returned file sample!");
                ReturnedFileDownload(browser, "XXX");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Empty()
        {
            RunInAllBrowsers(browser => {
                ReturnedFileDownload(browser, "");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Inline()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
                browser.WaitUntilDotvvmInited();

                browser.First("textarea").SendKeys("hello world");
                browser.First("get-file-inline", SelectByDataUi).Click();

                browser.WaitFor(() => {
                    AssertUI.TextEquals(browser.First("pre"), "hello world");
                },5000);
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_IncludeCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
                browser.WaitUntilDotvvmInited();

                InitIncludedFileResponseCapture(browser);
                browser.First("textarea").SendKeys("included command");
                browser.First("include-file", SelectByDataUi).Click();
                browser.WaitForPostback();

                AssertUI.InnerTextEquals(browser.First("included-command-count", SelectByDataUi), "1");
                var downloadURL = WaitForIncludedDownloadUrl(browser);
                AssertReturnedFile(browser.GetAbsoluteUrl(downloadURL), "included command");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_IncludeStaticCommand()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
                browser.WaitUntilDotvvmInited();

                InitIncludedFileResponseCapture(browser);
                browser.First("textarea").SendKeys("included static command");
                browser.First("include-static-file", SelectByDataUi).Click();

                AssertUI.InnerTextEquals(browser.First("included-static-command-count", SelectByDataUi), "1");
                var downloadURL = WaitForIncludedDownloadUrl(browser);
                AssertReturnedFile(browser.GetAbsoluteUrl(downloadURL), "included static command");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_IncludeCommandInline()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
                browser.WaitUntilDotvvmInited();

                InitIncludedFileResponseCapture(browser);
                browser.First("textarea").SendKeys("included inline command");
                browser.First("include-inline-file", SelectByDataUi).Click();
                browser.WaitForPostback();

                var downloadURL = WaitForIncludedDownloadUrl(browser);
                Assert.Null(browser.GetJavaScriptExecutor().ExecuteScript("return window.includedReturnedFileDownload;"));
                AssertReturnedFile(browser.GetAbsoluteUrl(downloadURL), "included inline command");
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_IncludeInit()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample + "?includeOnInit=1");
                browser.WaitUntilDotvvmInited();

                var downloadURL = (string)browser.GetJavaScriptExecutor().ExecuteScript(@"
                    var viewModel = JSON.parse(document.getElementById('__dot_viewmodel_root').value);
                    return viewModel.customProperties._dotvvm_IncludedReturnedFiles[0].url;
                ");
                AssertReturnedFile(browser.GetAbsoluteUrl(downloadURL), "included init");
            });
        }

        private void ReturnedFileDownload(IBrowserWrapper browser, string fileContent)
        {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
            browser.WaitUntilDotvvmInited();

            var jsexec = browser.GetJavaScriptExecutor();
            jsexec.ExecuteScript("window.downloadURL = \"\";");
            jsexec.ExecuteScript("dotvvm.events.redirect.subscribe(function (args) { window.downloadURL = args.url; });");

            browser.First("textarea").SendKeys(fileContent);
            browser.First("get-file", SelectByDataUi).Click();
            browser.WaitForPostback();
            var downloadURL = (string)jsexec.ExecuteScript("return window.downloadURL;");
            Assert.NotEmpty(downloadURL);

            AssertReturnedFile(browser.GetAbsoluteUrl(downloadURL), fileContent);
        }

        private void InitIncludedFileResponseCapture(IBrowserWrapper browser)
        {
            browser.GetJavaScriptExecutor().ExecuteScript(@"
                window.includedReturnedFileUrl = '';
                window.includedReturnedFileDownload = '';
                window.captureIncludedReturnedFile = function(response) {
                    var files = response && response.customProperties && response.customProperties._dotvvm_IncludedReturnedFiles;
                    if (files && files.length) {
                        window.includedReturnedFileUrl = files[0].url;
                        window.includedReturnedFileDownload = files[0].download || null;
                    }
                };
                dotvvm.events.postbackResponseReceived.subscribe(function(e) {
                    window.captureIncludedReturnedFile(e.serverResponseObject);
                });
                dotvvm.events.staticCommandMethodInvoked.subscribe(function(e) {
                    window.captureIncludedReturnedFile(e.serverResponseObject);
                });
            ");
        }

        private string WaitForIncludedDownloadUrl(IBrowserWrapper browser)
        {
            var jsexec = browser.GetJavaScriptExecutor();
            browser.WaitFor(() => {
                Assert.NotEmpty((string)jsexec.ExecuteScript("return window.includedReturnedFileUrl;"));
            }, 5000);

            return (string)jsexec.ExecuteScript("return window.includedReturnedFileUrl;");
        }

        private void AssertReturnedFile(string downloadURL, string fileContent)
        {
            string returnedFile;
#pragma warning disable SYSLIB0014 // obsolete
            using (var client = new WebClient())
#pragma warning restore SYSLIB0014
            {
                returnedFile = client.DownloadString(downloadURL);
            }
            Assert.Equal(fileContent, returnedFile);
        }

        public ReturnedFileTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
