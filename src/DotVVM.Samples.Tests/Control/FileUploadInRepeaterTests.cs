using System;
using System.IO;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.DotVVM;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests.Control
{
    public class FileUploadInRepeaterTests : AppSeleniumTest
    {
        public FileUploadInRepeaterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Complex_FileUploadInRepeater_FileUploadInRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_FileUploadInRepeater_FileUploadInRepeater);
                browser.WaitUntilDotvvmInited();

                var tempPath = Path.GetTempFileName();
                File.WriteAllBytes(tempPath, Enumerable.Range(0, 255).Select(i => (byte)i).ToArray());

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "0");
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "1", failureMessage: "FileCount is not updated to '1'.");

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "0");

                DotVVMAssert.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 2), tempPath);

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "1");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                DotVVMAssert.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "2");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                try
                {
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    TestOutput.WriteLine(ex.ToString());
                }
            });
        }
    }
}
