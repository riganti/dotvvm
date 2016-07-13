using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Dotvvm.Samples.Tests;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.SeleniumCore;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class FileUploadTests : SeleniumTestBase
    {

        [TestMethod]
        [Timeout(120000)]
        public void Control_FileUpload()
        {
			AppDomain.CurrentDomain
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_FileUpload);
                browser.Wait(1000);

                // get existing files
                var existingFiles = browser.FindElements("li").Select(e => e.GetText()).ToList();
                browser.Wait(1000);

                // generate a sample file to upload
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, string.Join(",", Enumerable.Range(1, 100000)));

                // write the full path to the dialog
                browser.FileUploadDialogSelect(browser.First(".dotvvm-upload-button a"),tempFile);

                // wait for the file to be uploaded

                browser.WaitFor(() => browser.First(".dotvvm-upload-files").GetText() == "1 files", 60000,
                    "File was not uploaded in 1 min interval.");

                TestContext.WriteLine("The file was uploaded.");

                // submit
                browser.Click("input[type=button]");

                // verify the file is there present
                browser.WaitFor(
                    () =>
                        browser.First("ul").FindElements("li").FirstOrDefault(t => !existingFiles.Contains(t.GetText())) !=
                        null, 60000, "File was not uploaded correctly.");

                // delete the file
                var firstLi =
                    browser.First("ul").FindElements("li").FirstOrDefault(t => !existingFiles.Contains(t.GetText()));
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_FileUpload + "?delete=" + firstLi.GetText());

                // delete the temp file
                File.Delete(tempFile);
            });
        }

        // TODO: RenderSettings.Mode="Server"
        // TODO: FileUpload with UploadCompleted command


    }
}
