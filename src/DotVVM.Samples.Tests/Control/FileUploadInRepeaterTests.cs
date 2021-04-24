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
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_FileUploadInRepeater_FileUploadInRepeater);
                browser.Wait(1000);


                var tempPath = Path.GetTempFileName();
                File.WriteAllBytes(tempPath, Enumerable.Range(0, 255).Select(i => (byte)i).ToArray());

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "0");
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);

                browser.WaitFor(() => browser.ElementAt(".files-count", 0).GetInnerText() == "1", 10000, "FileCount is not updated to '1'.");

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "0");

                DotVVMAssertModified.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 2), tempPath);
                browser.Wait(6000);

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "1");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                DotVVMAssertModified.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);
                browser.Wait(6000);

                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 0), "2");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                AssertUI.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    //TODO log
                }
            });
        }
    }
}
