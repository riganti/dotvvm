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
    public class FileUploadTests : AppSeleniumTest
    {
        public FileUploadTests(ITestOutputHelper output) : base(output)
        {
        }
        [Fact]
        [Timeout(120000)]
        public void Control_FileUpload_FileUpload()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_FileUpload);

                // get existing files
                var existingFiles = browser.FindElements("li").Select(e => e.GetText()).ToList();

                // generate a sample file to upload
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, string.Join(",", Enumerable.Range(1, 100000)));

                // write the full path to the dialog

                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), tempFile);

                // wait for the file to be uploaded
                AssertUI.TextEquals(browser.First(".dotvvm-upload-files"), "1 files", failureMessage: "File was not uploaded in 1 min interval.");

                //TODO: TestContext.WriteLine("The file was uploaded.");

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

        [Fact()]
        [Timeout(120000)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_FileUpload_IsAllowedOrNot))]
        public void Control_FileUpload_IsAllowedOrNot_IsFileAllowed()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_IsAllowedOrNot);
                

                var isFileTypeAllowed = browser.Single("span.isFileTypeAllowed");
                var isMaxSizeExceeded = browser.Single("span.isMaxSizeExceeded");

                var textFile = CreateTempFile("txt", 1);
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), textFile);

                AssertUI.TextEquals(browser.First(".dotvvm-upload-files"),"1 files",failureMessage: "File was not uploaded in 1 min interval.");

                AssertUI.TextEquals(isFileTypeAllowed, "true");
                AssertUI.TextEquals(isMaxSizeExceeded, "false");

                File.Delete(textFile);
            });
        }

        [Fact]
        [Timeout(120000)]
        public void Control_FileUpload_IsFileNotAllowed()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_IsAllowedOrNot);

                var isFileTypeAllowed = browser.Single("span.isFileTypeAllowed");
                var isMaxSizeExceeded = browser.Single("span.isMaxSizeExceeded");

                var mdFile = CreateTempFile("md", 1);
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), mdFile);

                AssertUI.TextEquals(browser.First(".dotvvm-upload-files"), "1 files", failureMessage: "File was not uploaded in 1 min interval.");

                AssertUI.TextEquals(isFileTypeAllowed, "false");
                AssertUI.TextEquals(isMaxSizeExceeded, "false");

                File.Delete(mdFile);
            });
        }

        [Fact]
        [Timeout(120000)]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_FileUpload_IsAllowedOrNot))]
        public void Control_FileUpload_IsAllowedOrNot_FileTooLarge()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_IsAllowedOrNot);

                var isFileTypeAllowed = browser.Single("span.isFileTypeAllowed");
                var isMaxSizeExceeded = browser.Single("span.isMaxSizeExceeded");

                var largeFile = CreateTempFile("txt", 2);
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), largeFile);

                AssertUI.TextEquals(browser.First(".dotvvm-upload-files"), "1 files", failureMessage: "File was not uploaded in 1 min interval.");

                AssertUI.TextEquals(isFileTypeAllowed, "true");
                AssertUI.TextEquals(isMaxSizeExceeded, "true");

                File.Delete(largeFile);
            });
        }

        [Fact]
        [Timeout(120000)]
        public void Control_FileUpload_FileSize()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_FileSize);

                var fileSize = browser.Single("span.fileSize");

                var file = CreateTempFile("txt", 2);
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), file);

                AssertUI.TextEquals(browser.First(".dotvvm-upload-files"), "1 files", failureMessage: "File was not uploaded in 1 min interval.");

                AssertUI.TextEquals(fileSize, "2 MB");

                File.Delete(file);
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

        [Fact]
        public void Control_FileUpload_PasteDrop_InitialState()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_PasteDrop);

                // Verify initial state
                var textBox = browser.Single("textarea");
                AssertUI.IsDisplayed(textBox);

                // Verify files count is 0
                var filesCountParagraph = browser.FindElements("p").Last();
                AssertUI.TextEquals(filesCountParagraph, "Number of uploaded files: 0");

                // Verify the repeater is empty initially
                var items = browser.FindElements("ul li");
                items.ThrowIfDifferentCountThan(0);

                // Verify error is empty
                var errorParagraph = browser.ElementAt("p", 0);
                AssertUI.TextEquals(errorParagraph, "Error:");

                // Verify busy is not visible
                var busyElements = browser.FindElements("p").Where(p => p.GetText().Contains("busy"));
                Assert.Empty(busyElements);
            });
        }

        [Fact]
        public void Control_FileUpload_PasteDrop_TextBoxHasBinding()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_PasteDrop);

                var textBox = browser.Single("textarea");

                // Verify the textarea has the dotvvm-FileUpload-UploadOnPasteOrDrop binding
                var bindingAttribute = textBox.GetAttribute("data-bind");
                Assert.Contains("dotvvm-FileUpload-UploadOnPasteOrDrop", bindingAttribute);

                // Verify the textarea has upload completed handler
                var uploadCompletedAttribute = textBox.GetAttribute("data-dotvvm-upload-completed");
                Assert.NotNull(uploadCompletedAttribute);
            });
        }

    }
}
