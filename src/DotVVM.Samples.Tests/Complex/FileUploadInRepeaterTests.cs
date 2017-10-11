using System;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System.IO;
using System.Linq;
using Riganti.Utils.Testing.Selenium.DotVVM;

namespace DotVVM.Samples.Tests.Complex
{
    [TestClass]
    public class FileUploadInRepeaterTests : AppSeleniumTest
    {
        [TestMethod]
        public void Complex_FileUploadInRepeater_FileUploadInRepeater()
        {
            //TODO Rewrite FileUpload in selenium api
            throw new NotImplementedException();
            //RunInAllBrowsers(browser =>
            //{
            //    browser.NavigateToUrl(SamplesRouteUrls.ComplexSamples_FileUploadInRepeater_FileUploadInRepeater);
            //    browser.Wait(1000);

            //    // generate temp file
            //    var tempPath = Path.GetTempFileName();
            //    File.WriteAllBytes(tempPath, Enumerable.Range(0, 255).Select(i => (byte)i).ToArray());

            //    // upload file in the first part
            //    browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("0");
            //    browser.ElementAt(".dotvvm-upload-button a", 0).UploadFile(tempPath);

            //    browser.WaitFor(() => browser.ElementAt(".files-count", 0).GetInnerText() == "1", 10000, "FileCount is not updated to '1'.");
            //    browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
            //    browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("0");

            //    // upload file in the third part

            //    var e = new ElementWrapper(null, null);
            //    e.UploadFile(tempPath);
            //    browser.ElementAt(".dotvvm-upload-button a", 2).UploadFile(tempPath);
            //    browser.Wait(6000);

            //    browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("1");
            //    browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
            //    browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("1");

            //    // upload file in the first part
            //    browser.ElementAt(".dotvvm-upload-button a", 0).UploadFile(tempPath);
            //    browser.Wait(6000);
            //    browser.ElementAt(".files-count", 0).CheckIfInnerTextEquals("2");
            //    browser.ElementAt(".files-count", 1).CheckIfInnerTextEquals("0");
            //    browser.ElementAt(".files-count", 2).CheckIfInnerTextEquals("1");

            //    // try to delete temp file
            //    try
            //    {
            //        File.Delete(tempPath);
            //    }
            //    catch(Exception ex)
            //    {
            //        Log(ex);
            //    }
            //});
        }
    }
}