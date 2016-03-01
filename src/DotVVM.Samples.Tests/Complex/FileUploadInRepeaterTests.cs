using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class FileUploadInRepeaterTests : SeleniumTestBase
    {

        [TestMethod]
        public void Complex_FileUploadInRepeater()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_FileUploadInRepeater_FileUploadInRepeater);

                // generate temp file
                var tempPath = Path.GetTempFileName();
                File.WriteAllBytes(tempPath, Enumerable.Range(0, 255).Select(i => (byte)i).ToArray());

                // upload file in the first part
                browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("0");
                browser.FileUploadDialogSelect(browser.ElementAt(".dot-upload-button a", 0), tempPath);
                browser.Wait(3000);
                browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("1");
                browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
                browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("0");

                // upload file in the third part
                browser.FileUploadDialogSelect(browser.ElementAt(".dot-upload-button a", 2), tempPath);
                browser.Wait(3000);
                browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("1");
                browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
                browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("1");

                // upload file in the first part
                browser.FileUploadDialogSelect(browser.ElementAt(".dot-upload-button a", 0), tempPath);
                browser.Wait(3000);
                browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("2");
                browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
                browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("1");

                // try to delete temp file
                try
                {
                    File.Delete(tempPath);
                }
                catch { }
            });
        }
    }
}
