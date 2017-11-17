
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;

namespace DotVVM.Samples.Tests.Feature
{
    [TestClass]
    public class ReturnedFileTests : AppSeleniumTest
    {

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Simple()
        {
            RunInAllBrowsers(browser => {
                ReturnedFileDownload(browser, "Hello DotVVM returned file sample!");
            });
        }

        [TestMethod]
        [SampleReference(nameof(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample))]
        public void Feature_ReturnedFile_ReturnedFileSample_Empty()
        {
            RunInAllBrowsers(browser =>
            {
                ReturnedFileDownload(browser, "");
            });
        }

        private void ReturnedFileDownload(IBrowserWrapperFluentApi browser, string fileContent)
        {
            browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ReturnedFile_ReturnedFileSample);
            var jsexec = browser.GetJavaScriptExecutor();
            jsexec.ExecuteScript("var downloadURL = \"\";");
            jsexec.ExecuteScript("DotVVM.prototype.performRedirect = function(url){downloadURL = url};");

            browser.First("textarea").SendKeys(fileContent);
            browser.First("input").SendKeys(Keys.Enter);
            var downloadURL = (string)jsexec.ExecuteScript("return downloadURL");
            Assert.IsNotNull(downloadURL);

            string returnedFile;
            using (var client = new WebClient())
            {
                returnedFile = client.DownloadString(browser.GetAbsoluteUrl(downloadURL));
            }
            Assert.AreEqual(fileContent, returnedFile);
        }
    }
}
