using System.Net;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
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

                browser.First("textarea").SendKeys("hello world");
                browser.Last("input[type=button]").Click();
                browser.WaitForPostback();

                AssertUI.TextEquals(browser.First("pre"), "hello world");
            });
        }

        private void ReturnedFileDownload(IBrowserWrapper browser, string fileContent)
        {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
            var jsexec = browser.GetJavaScriptExecutor();
            jsexec.ExecuteScript("window.downloadURL = \"\";");
            jsexec.ExecuteScript("dotvvm.events.redirect.subscribe(function (args) { window.downloadURL = args.url; });");

            browser.First("textarea").SendKeys(fileContent);
            browser.First("input").SendKeys(Keys.Enter);
            browser.WaitForPostback();
            var downloadURL = (string)jsexec.ExecuteScript("return window.downloadURL;");
            Assert.False(string.IsNullOrEmpty(downloadURL));

            string returnedFile;
            using (var client = new WebClient())
            {
                returnedFile = client.DownloadString(browser.GetAbsoluteUrl(downloadURL));
            }
            Assert.Equal(fileContent, returnedFile);
        }

        public ReturnedFileTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
