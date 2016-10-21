using System.IO;
using System.Linq;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class FileUploadTests : SeleniumTestBase
    {
        [TestMethod]
        [Timeout(120000)]
        public void Control_FileUpload()
        {
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
                browser.FileUploadDialogSelect(browser.First(".dotvvm-upload-button a"), tempFile);

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

        [TestMethod]
        [Timeout(120000)]
        public void Control_FileUpload_FileTypeAllowed()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_AllowedOrNot);
                browser.Wait(1000);

                var fileTypeAllowed = browser.Single("span.fileTypeAllowed");
                var maxSizeExceeded = browser.Single("span.maxSizeExceeded");
                
                var textFile = CreateTempFile("txt", 1);
                browser.FileUploadDialogSelect(browser.First(".dotvvm-upload-button a"), textFile);

                browser.WaitFor(() => browser.First(".dotvvm-upload-files").GetText() == "1 files", 60000,
                    "File was not uploaded in 1 min interval.");

                fileTypeAllowed.CheckIfTextEquals("true");
                maxSizeExceeded.CheckIfTextEquals("false");

                File.Delete(textFile);
            });
        }

        [TestMethod]
        [Timeout(120000)]
        public void Control_FileUpload_FileTypeNotAllowed()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_AllowedOrNot);
                browser.Wait(1000);

                var fileTypeAllowed = browser.Single("span.fileTypeAllowed");
                var maxSizeExceeded = browser.Single("span.maxSizeExceeded");

                var mdFile = CreateTempFile("md", 1);
                browser.FileUploadDialogSelect(browser.First(".dotvvm-upload-button a"), mdFile);

                browser.WaitFor(() => browser.First(".dotvvm-upload-files").GetText() == "1 files", 60000,
                    "File was not uploaded in 1 min interval.");

                fileTypeAllowed.CheckIfTextEquals("false");
                maxSizeExceeded.CheckIfTextEquals("false");

                File.Delete(mdFile);
            });
        }

        [TestMethod]
        [Timeout(120000)]
        public void Control_FileUpload_FileTooLarge()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_AllowedOrNot);
                browser.Wait(1000);

                var fileTypeAllowed = browser.Single("span.fileTypeAllowed");
                var maxSizeExceeded = browser.Single("span.maxSizeExceeded");

                var largeFile = CreateTempFile("txt", 2);
                browser.FileUploadDialogSelect(browser.First(".dotvvm-upload-button a"), largeFile);

                browser.WaitFor(() => browser.First(".dotvvm-upload-files").GetText() == "1 files", 60000,
                    "File was not uploaded in 1 min interval.");

                fileTypeAllowed.CheckIfTextEquals("true");
                maxSizeExceeded.CheckIfTextEquals("true");

                File.Delete(largeFile);
            });
        }

        private string CreateTempFile(string extension, long size)
        {
            var tempFile = Path.GetTempFileName();
            tempFile = Path.ChangeExtension(tempFile, extension);

            using (var fs = new FileStream(tempFile, FileMode.CreateNew))
            {
                fs.SetLength(size * 1024 * 1024);
            }

            return tempFile;
        }

        // TODO: FileUpload with UploadCompleted command

        // TODO: RenderSettings.Mode="Server"
    }
}