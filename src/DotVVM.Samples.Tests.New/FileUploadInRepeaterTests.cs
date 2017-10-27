using DotVVM.Testing.Abstractions;
using Riganti.Utils.Testing.Selenium.Core;
using Riganti.Utils.Testing.Selenium.DotVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Assert = Riganti.Utils.Testing.Selenium.Core.Assert;

namespace DotVVM.Samples.Tests.New
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

                Assert.InnerTextEquals(browser.ElementAt(".files-count", 0), "0");
                ElementWrapperExtensions.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);

                browser.WaitFor(() => browser.ElementAt(".files-count", 0).GetInnerText() == "1", 10000, "FileCount is not updated to '1'.");

                Assert.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                Assert.InnerTextEquals(browser.ElementAt(".files-count", 2), "0");

                var e = new ElementWrapper(null, null);
                e.UploadFile(tempPath);
                ElementWrapperExtensions.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 2), tempPath);
                browser.Wait(6000);

                Assert.InnerTextEquals(browser.ElementAt(".files-count", 0), "1");
                Assert.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                Assert.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                ElementWrapperExtensions.UploadFile((ElementWrapper)browser.ElementAt(".dotvvm-upload-button a", 0), tempPath);
                browser.Wait(6000);

                Assert.InnerTextEquals(browser.ElementAt(".files-count", 0), "2");
                Assert.InnerTextEquals(browser.ElementAt(".files-count", 1), "0");
                Assert.InnerTextEquals(browser.ElementAt(".files-count", 2), "1");

                try
                {
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    //TODO log
                }

            });
        }
    }
}
