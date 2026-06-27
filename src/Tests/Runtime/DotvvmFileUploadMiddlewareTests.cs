using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using Microsoft.AspNet.WebUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DotvvmFileUploadMiddlewareTests
    {
        [TestMethod]
        public async Task StoreFile_ReportsSizeAfterReadingSection()
        {
            var fileBytes = new byte[] { 1, 2, 3, 4, 5 };
            var storage = new TestUploadedFileStorage();
            var section = await CreateMultipartSection(fileBytes);

            Assert.AreEqual("Microsoft.AspNet.WebUtilities.MultipartReaderStream", section.Body.GetType().FullName);
            Assert.AreEqual(0, section.Body.Length);

            var result = await StoreFile(
                DotvvmTestHelper.CreateContext(),
                section,
                storage,
                new FileUploadToken());

            Assert.AreEqual(fileBytes.Length, result.FileSize.Bytes);
            Assert.AreEqual(fileBytes.Length, storage.StoredBytes.Length);
        }

        [TestMethod]
        public async Task StoreFile_MaxFileSizeExceeded_RejectsRequest()
        {
            var context = DotvvmTestHelper.CreateContext();
            var section = await CreateMultipartSection([1, 2, 3, 4, 5]);

            var storage = new TestUploadedFileStorage();
            Assert.AreEqual(typeof(MultipartReaderStream), section.Body.GetType());

            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() =>
                StoreFile(
                    context,
                    section,
                    storage,
                    new FileUploadToken { MaxFileSize = 4 }));

            Assert.AreEqual(413, context.HttpContext.Response.StatusCode);
            Assert.AreEqual(0, storage.StoredBytes.Length);
        }

        private static Task<UploadedFile> StoreFile(IDotvvmRequestContext context, MultipartSection section, IUploadedFileStorage fileStorage, FileUploadToken token)
        {
            var middleware = new DotvvmFileUploadMiddleware(
                outputRenderer: null!,
                fileStorage: null!,
                viewModelSerializer: null!,
                csrfProtector: null!,
                viewModelProtector: null!);
            var method = typeof(DotvvmFileUploadMiddleware).GetMethod("StoreFile", BindingFlags.Instance | BindingFlags.NonPublic)!;
            return (Task<UploadedFile>)method.Invoke(middleware, new object[] { context, section, fileStorage, token })!;
        }

        private static async Task<MultipartSection> CreateMultipartSection(byte[] fileBytes)
        {
            const string boundary = "dotvvm-test-boundary";
            byte[] body = [
                ..StringUtils.Utf8.GetBytes(
                    $"--{boundary}\r\n" +
                    "Content-Disposition: form-data; name=\"upload\"; filename=\"test.txt\"\r\n" +
                    "Content-Type: text/plain\r\n" +
                    "\r\n"
                ),
                ..fileBytes,
                ..StringUtils.Utf8.GetBytes($"\r\n--{boundary}--\r\n")
            ];

            var reader = new MultipartReader(boundary, MakeNonSeekable(body));
            return (await reader.ReadNextSectionAsync())!;
        }

        static void WriteAscii(Stream stream, string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        static Stream MakeNonSeekable(byte[] data)
        {
#if DotNetCore
            var pipe = new System.IO.Pipelines.Pipe();
            _ = pipe.Writer.WriteAsync(data);
            pipe.Writer.Complete();
            return pipe.Reader.AsStream();
#else
            // IO.Pipelines not supported, we won't test the non-seekable part
            return new MemoryStream(data);
#endif
        }

        private sealed class TestUploadedFileStorage : IUploadedFileStorage
        {
            public byte[] StoredBytes { get; private set; } = Array.Empty<byte>();

            public async Task<Guid> StoreFileAsync(Stream stream)
            {
                using (var buffer = new MemoryStream())
                {
                    await stream.CopyToAsync(buffer);
                    StoredBytes = buffer.ToArray();
                }
                return Guid.NewGuid();
            }

            public Task DeleteFileAsync(Guid fileId) => Task.CompletedTask;

            public Task<Stream> GetFileAsync(Guid fileId) => Task.FromResult<Stream>(new MemoryStream(StoredBytes));
        }
    }
}
