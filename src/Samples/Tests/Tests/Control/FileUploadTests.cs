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

                var error = browser.Single("span.upload-error");

                var largeFile = CreateTempFile("txt", 2);
                DotVVMAssertModified.UploadFile((ElementWrapper)browser.First(".dotvvm-upload-button a"), largeFile);

                AssertUI.TextEquals(error, "Uploaded file is too large.");

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

        [Theory]
        [InlineData("paste", "large.txt", 1024 * 1024 + 1, "Uploaded file is too large.")]
        [InlineData("drop", "large.txt", 1024 * 1024 + 1, "Uploaded file is too large.")]
        [InlineData("paste", "file.md", 1, "Uploaded file type is not allowed.")]
        [InlineData("drop", "file.md", 1, "Uploaded file type is not allowed.")]
        public void Control_FileUpload_PasteDrop_InvalidFile(string eventType, string fileName, int size, string error)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_PasteDrop);
                browser.GetJavaScriptExecutor().ExecuteScript(@"
                    const originalSend = XMLHttpRequest.prototype.send;
                    window.uploadRequests = 0;
                    XMLHttpRequest.prototype.send = function(body) {
                        if (body instanceof FormData) window.uploadRequests++;
                        return originalSend.apply(this, arguments);
                    };
                    const transfer = new DataTransfer();
                    transfer.items.add(new File([new Uint8Array(arguments[2])], arguments[1], { type: 'text/plain' }));
                    const event = arguments[0] === 'paste'
                        ? new ClipboardEvent('paste', { clipboardData: transfer, bubbles: true, cancelable: true })
                        : new DragEvent('drop', { dataTransfer: transfer, bubbles: true, cancelable: true });
                    if (arguments[0] === 'paste') Object.defineProperty(event, 'clipboardData', { value: transfer });
                    document.querySelector('textarea').dispatchEvent(event);
                ", eventType, fileName, size);

                AssertUI.TextEquals(browser.ElementAt("p", 0), "Error: " + error);
                AssertUI.TextEquals(browser.FindElements("p").Last(), "Number of uploaded files: 0");
                browser.FindElements("ul li").ThrowIfDifferentCountThan(0);
                Assert.Equal(0L, browser.GetJavaScriptExecutor().ExecuteScript("return window.uploadRequests;"));
            });
        }

        [Theory]
        [InlineData("paste")]
        [InlineData("drop")]
        public void Control_FileUpload_PasteDrop_AllowedFile(string eventType)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_FileUpload_PasteDrop);
                browser.GetJavaScriptExecutor().ExecuteScript(@"
                    const transfer = new DataTransfer();
                    transfer.items.add(new File(['file contents'], 'file.txt', { type: 'text/plain' }));
                    const event = arguments[0] === 'paste'
                        ? new ClipboardEvent('paste', { clipboardData: transfer, bubbles: true, cancelable: true })
                        : new DragEvent('drop', { dataTransfer: transfer, bubbles: true, cancelable: true });
                    if (arguments[0] === 'paste') Object.defineProperty(event, 'clipboardData', { value: transfer });
                    document.querySelector('textarea').dispatchEvent(event);
                ", eventType);

                AssertUI.TextEquals(browser.FindElements("p").Last(), "Number of uploaded files: 1");
                AssertUI.TextEquals(browser.ElementAt("p", 0), "Error:");
                browser.FindElements("ul li").ThrowIfDifferentCountThan(1);
                Assert.Contains("file.txt", browser.Single("ul li").GetText());
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
