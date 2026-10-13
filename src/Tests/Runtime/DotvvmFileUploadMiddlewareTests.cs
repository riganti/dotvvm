using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
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

        [TestMethod]
        public async Task ChunkUpload_IntermediateTokenAndFinalResultRetainMetadataAndCumulativeSize()
        {
            var storage = new TestChunkStorage();
            var token = new FileUploadToken { MaxFileSize = 5, AllowedFileTypes = ".txt" };
            var firstContext = CreateChunkContext([1, 2], false, totalSize: 5);
            var firstResults = await SaveChunk(firstContext, storage, token);
            Assert.AreEqual(0, firstResults.Count);
            var protectedToken = firstContext.HttpContext.Response.Headers["X-DotVVM-UploadToken"];
            Assert.IsFalse(string.IsNullOrEmpty(protectedToken));
            var unprotected = new DotvvmTestHelper.FakeProtector().Unprotect(Convert.FromBase64String(protectedToken!), "FileUpload", "");
            var continuation = JsonSerializer.Deserialize<FileUploadToken>(unprotected)!;
            Assert.AreEqual(storage.Id, continuation.FileId);
            Assert.AreEqual(2L, continuation.NextPosition);
            Assert.AreEqual(5L, continuation.MaxFileSize);
            Assert.AreEqual(5L, continuation.TotalFileSize);
            Assert.AreEqual(".txt", continuation.AllowedFileTypes);
            Assert.AreEqual("test.txt", continuation.FileName);
            Assert.AreEqual("text/plain", continuation.MimeType);

            var finalContext = CreateChunkContext([3, 4, 5], true);
            var finalResults = await SaveChunk(finalContext, storage, continuation);
            Assert.AreEqual(1, finalResults.Count);
            Assert.AreEqual(storage.Id, finalResults[0].FileId);
            Assert.AreEqual(5L, finalResults[0].FileSize.Bytes);
            Assert.IsTrue(finalResults[0].IsFileTypeAllowed);
            Assert.IsTrue(string.IsNullOrEmpty(finalContext.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, storage.StoredBytes);
        }

        [TestMethod]
        public async Task ChunkUpload_CumulativeAndChunkLimitsRejectBeforeStorageMutation()
        {
            var storage = new TestChunkStorage();
            await SaveChunk(CreateChunkContext([1, 2], false, totalSize: 4), storage, new FileUploadToken { MaxFileSize = 4 });
            var token = new FileUploadToken { FileId = storage.Id, NextPosition = 2, TotalFileSize = 4, MaxFileSize = 4, FileName = "test.txt", MimeType = "text/plain" };
            var context = CreateChunkContext([3, 4, 5], true);
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(context, storage, token));
            Assert.AreEqual(413, context.HttpContext.Response.StatusCode);
            Assert.AreEqual(2, storage.StoredBytes.Length);

            var chunkContext = CreateChunkContext([1, 2, 3], true, chunkSize: 2, totalSize: 3);
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(chunkContext, storage, new FileUploadToken()));
            Assert.AreEqual(413, chunkContext.HttpContext.Response.StatusCode);
            Assert.AreEqual(2, storage.StoredBytes.Length);
        }

        [TestMethod]
        public async Task ChunkUpload_RejectsChangedMetadataEmptyContinuationAndExtraSections()
        {
            var storage = new TestChunkStorage();
            await SaveChunk(CreateChunkContext([1], false, totalSize: 2), storage, new FileUploadToken());
            var token = new FileUploadToken { FileId = storage.Id, NextPosition = 1, TotalFileSize = 2, FileName = "other.txt", MimeType = "text/plain" };
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(CreateChunkContext([2], true), storage, token));
            token.FileName = "test.txt";
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(CreateChunkContext([], true), storage, token));
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(CreateChunkContext([], false, totalSize: 1), storage, new FileUploadToken()));
            var extra = CreateChunkContext([2], false, extraSection: true);
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(extra, storage, token));
            Assert.IsTrue(string.IsNullOrEmpty(extra.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
            Assert.AreEqual(1, storage.StoredBytes.Length);
        }

        [TestMethod]
        public async Task ChunkUpload_EmptyInitialFinalIsAllowedAndDisallowedTypeIsReported()
        {
            var storage = new TestChunkStorage();
            var results = await SaveChunk(CreateChunkContext([], true, totalSize: 0), storage, new FileUploadToken { AllowedFileTypes = "image/*" });
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0L, results[0].FileSize.Bytes);
            Assert.IsFalse(results[0].IsFileTypeAllowed);
            Assert.AreEqual(1, storage.StoreCalls);
        }

        [TestMethod]
        public async Task ChunkUpload_NegativeMaxSizePreservesLegacyUnlimitedSemantics()
        {
            var token = new FileUploadToken { MaxFileSize = -1 };
            var legacy = await StoreFile(DotvvmTestHelper.CreateContext(), await CreateMultipartSection([1, 2, 3]), new TestUploadedFileStorage(), token);
            var chunks = await SaveChunk(CreateChunkContext([1, 2, 3], true, totalSize: 3), new TestChunkStorage(), token);
            Assert.AreEqual(3L, legacy.FileSize.Bytes);
            Assert.AreEqual(3L, chunks[0].FileSize.Bytes);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("-1")]
        [DataRow("+1")]
        [DataRow(" 1")]
        [DataRow("1 ")]
        [DataRow("1.0")]
        [DataRow("1e0")]
        [DataRow("1,0")]
        [DataRow("١")]
        [DataRow("9223372036854775808")]
        public async Task ChunkUpload_RejectsMissingOrMalformedInitialTotal(string total)
        {
            var context = CreateChunkContext([1], true);
            context.HttpContext.Request.Headers["X-DotVVM-UploadFileSize"] = total;
            var storage = new TestChunkStorage();
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(context, storage, new FileUploadToken()));
            Assert.AreEqual(0, storage.StoreCalls);
            Assert.IsTrue(string.IsNullOrEmpty(context.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
        }

        [DataTestMethod]
        [DataRow("1", true)]
        [DataRow("01", true)]
        [DataRow("9223372036854775807", false)]
        public async Task ChunkUpload_AcceptsInvariantNonnegativeLongTotal(string total, bool final)
        {
            var context = CreateChunkContext([1], final);
            context.HttpContext.Request.Headers["X-DotVVM-UploadFileSize"] = total;
            var token = new FileUploadToken();
            await SaveChunk(context, new TestChunkStorage(), token);
            Assert.AreEqual(long.Parse(total, CultureInfo.InvariantCulture), token.TotalFileSize);
        }

        [TestMethod]
        public void UploadToken_OmitsNullTotalFromInitialRendering()
        {
            var json = JsonSerializer.Serialize(new FileUploadToken());
            Assert.IsFalse(json.Contains(nameof(FileUploadToken.TotalFileSize)));
        }

        [TestMethod]
        public async Task ChunkUpload_DeclaredTotalExceedingMaxRejectsBeforeReadingOrStorage()
        {
            var context = CreateChunkContext([1], false, totalSize: 5);
            var storage = new TestChunkStorage();
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() =>
                SaveChunk(context, storage, new FileUploadToken { MaxFileSize = 4 }));
            Assert.AreEqual(413, context.HttpContext.Response.StatusCode);
            Assert.AreEqual(0L, context.HttpContext.Request.Body.Position);
            Assert.AreEqual(0, storage.StoreCalls);
            Assert.IsTrue(string.IsNullOrEmpty(context.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
        }

        [DataTestMethod]
        [DataRow(true, 2L, 1, true)]
        [DataRow(true, 1L, 1, false)]
        [DataRow(true, 0L, 0, false)]
        [DataRow(true, 0L, 1, true)]
        [DataRow(true, 1L, 2, true)]
        [DataRow(true, 1L, 2, false)]
        [DataRow(false, 3L, 1, true)]
        [DataRow(false, 2L, 1, false)]
        [DataRow(false, 2L, 2, true)]
        [DataRow(false, 2L, 2, false)]
        public async Task ChunkUpload_WrongFinalFlagsAndExtensionsNeverMutateStorage(bool initial, long total, int length, bool final)
        {
            var storage = new TestChunkStorage();
            var token = new FileUploadToken();
            if (!initial)
                await SaveChunk(CreateChunkContext([1], false, totalSize: total), storage, token);
            var context = CreateChunkContext(new byte[length], final, totalSize: initial ? total : null);
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(context, storage, token));
            Assert.AreEqual(initial ? 0 : 1, storage.StoreCalls);
            CollectionAssert.AreEqual(initial ? Array.Empty<byte>() : new byte[] { 1 }, storage.StoredBytes);
            Assert.IsTrue(string.IsNullOrEmpty(context.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
            Assert.AreEqual(initial ? 0L : 1L, token.NextPosition);
        }

        [DataTestMethod]
        [DataRow(null, 1L, null)]
        [DataRow(-1L, 1L, null)]
        [DataRow(0L, 1L, null)]
        [DataRow(0L, 0L, null)]
        [DataRow(1L, 1L, null)]
        [DataRow(1L, 2L, null)]
        [DataRow(2L, 1L, "2")]
        [DataRow(2L, 1L, "100")]
        public async Task ChunkUpload_RejectsMissingInvalidCompletedOrOverriddenProtectedTotal(long? total, long position, string overrideTotal)
        {
            var storage = new TestChunkStorage();
            await SaveChunk(CreateChunkContext([1], false, totalSize: 2), storage, new FileUploadToken());
            var token = new FileUploadToken {
                FileId = storage.Id, NextPosition = position, TotalFileSize = total,
                FileName = "test.txt", MimeType = "text/plain"
            };
            var context = CreateChunkContext([2], true);
            context.HttpContext.Request.Headers["X-DotVVM-UploadFileSize"] = overrideTotal;
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => SaveChunk(context, storage, token));
            CollectionAssert.AreEqual(new byte[] { 1 }, storage.StoredBytes);
            Assert.AreEqual(1, storage.StoreCalls);
            Assert.IsTrue(string.IsNullOrEmpty(context.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
        }

        [TestMethod]
        public async Task LegacyUpload_RejectsContinuationToken()
        {
            var storage = new TestChunkStorage();
            var context = CreateChunkContext([1], true);
            context.HttpContext.Request.Headers["X-DotVVM-UploadChunk"] = "";
            var middleware = CreateMiddleware(storage);
            var method = typeof(DotvvmFileUploadMiddleware).GetMethod("SaveFiles", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var boundary = System.Text.RegularExpressions.Regex.Match("dotvvm-test-boundary", "(.*)").Groups[1];
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(async () =>
                await (Task)method.Invoke(middleware, new object[] { context, boundary, new List<UploadedFile>(), new FileUploadToken { FileId = storage.Id, NextPosition = 1 } })!);
            Assert.AreEqual(0, storage.StoredBytes.Length);
        }

        [TestMethod]
        public void ChunkConfiguration_DefaultValidationAndFreezing()
        {
            var configuration = new DotvvmRuntimeConfiguration();
            Assert.AreEqual(3 * 1024 * 1024, configuration.FileUploadChunkSizeBytes);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => configuration.FileUploadChunkSizeBytes = -1);
            configuration.FileUploadChunkSizeBytes = 0;
            configuration.Freeze();
            Assert.ThrowsException<FreezableUtils.ObjectIsFrozenException>(() => configuration.FileUploadChunkSizeBytes = 1);
        }

        [TestMethod]
        public void ChunkConfiguration_SerializesApplicationSetting()
        {
            var config = DotvvmTestHelper.CreateConfiguration();
            config.Runtime.FileUploadChunkSizeBytes = 1234;
            using var json = JsonDocument.Parse(VisualStudioHelper.SerializeConfig(config, false));
            Assert.AreEqual(1234, json.RootElement.GetProperty("config").GetProperty("runtime").GetProperty("fileUploadChunkSizeBytes").GetInt32());
        }

        [DataTestMethod]
        [DataRow(true, 3145728, 3145728)]
        [DataRow(true, 1234, 1234)]
        [DataRow(true, 0, 0)]
        [DataRow(false, 3145728, 0)]
        public async Task ChunkRendering_BothBindingsAdvertiseSupportedChunkSize(bool supported, int configured, int expected)
        {
            var helper = new ControlTestHelper(
                config: c => c.Runtime.FileUploadChunkSizeBytes = configured,
                services: s => s.Services.AddSingleton<IUploadedFileStorage>(
                    supported ? new TestChunkStorage() : new TestUploadedFileStorage()));
            var page = await helper.RunPage(typeof(ChunkUploadViewModel), """
                <dot:FileUpload UploadedFiles={value: Files} />
                <dot:TextBox Text={value: Text} FileUpload.UploadOnPasteOrDrop={value: Files} />
                """);
            var upload = page.Html.QuerySelector("input[type=file]")!.GetAttribute("data-bind")!;
            var paste = page.Html.QuerySelector("input[type=text]")!.GetAttribute("data-bind")!;
            StringAssert.Contains(upload, "\"chunkSize\":" + expected);
            StringAssert.Contains(paste, "chunkSize: " + expected);
        }

        public class ChunkUploadViewModel
        {
            public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();
            public string Text { get; set; } = "";
        }

#if DotNetCore
        [TestMethod]
        public async Task Handle_ProtectedContinuationCompletesWithStaticCommandResult()
        {
            var storage = new TestChunkStorage();
            var config = CreateProtectedUploadConfiguration(storage);
            var first = CreateHandleContext(config, [1, 2], false, "alice", totalSize: 3);
            first.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = FileUpload.GetFileUploadToken(first, ".txt", 1);
            var middleware = DotvvmFileUploadMiddleware.TryCreate(first.Services)!;
            Assert.IsTrue(await middleware.Handle(first));
            using (var json = ReadResponseJson(first))
                Assert.AreEqual(0, json.RootElement.GetProperty("result").GetArrayLength());
            var continuation = first.HttpContext.Response.Headers["X-DotVVM-UploadToken"];
            Assert.IsFalse(string.IsNullOrEmpty(continuation));
            var protectedMetadata = JsonSerializer.Deserialize<FileUploadToken>(
                first.Services.GetRequiredService<IViewModelProtector>().Unprotect(
                    Convert.FromBase64String(continuation!), "FileUpload", ProtectionHelpers.GetUserIdentity(first)))!;
            Assert.AreEqual(3L, protectedMetadata.TotalFileSize);
            Assert.AreEqual(2L, protectedMetadata.NextPosition);

            var nonfinalAtEnd = CreateHandleContext(config, [3], false, "alice");
            nonfinalAtEnd.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => middleware.Handle(nonfinalAtEnd));
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, storage.StoredBytes);
            Assert.IsTrue(string.IsNullOrEmpty(nonfinalAtEnd.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));

            var extendedFinal = CreateHandleContext(config, [3, 4], true, "alice");
            extendedFinal.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            await Assert.ThrowsExceptionAsync<DotvvmInterruptRequestExecutionException>(() => middleware.Handle(extendedFinal));
            Assert.AreEqual(413, extendedFinal.HttpContext.Response.StatusCode);
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, storage.StoredBytes);
            Assert.IsTrue(string.IsNullOrEmpty(extendedFinal.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));

            var final = CreateHandleContext(config, [3], true, "alice");
            final.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            Assert.IsTrue(await middleware.Handle(final));
            using var finalJson = ReadResponseJson(final);
            var result = finalJson.RootElement.GetProperty("result");
            Assert.AreEqual(1, result.GetArrayLength());
            Assert.AreEqual(storage.Id, result[0].GetProperty("FileId").GetGuid());
            Assert.AreEqual(3L, result[0].GetProperty("FileSize").GetProperty("Bytes").GetInt64());
            Assert.IsTrue(string.IsNullOrEmpty(final.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, storage.StoredBytes);

            var replay = CreateHandleContext(config, [4], true, "alice");
            replay.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            Assert.IsTrue(await middleware.Handle(replay));
            StringAssert.StartsWith(replay.HttpContext.Response.ContentType, "text/plain");
            Assert.IsTrue(string.IsNullOrEmpty(replay.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, storage.StoredBytes);
        }

        [TestMethod]
        public async Task Handle_ContinuationRejectsDifferentIdentityAndTamperingWithoutStorageMutation()
        {
            var storage = new TestChunkStorage();
            var config = CreateProtectedUploadConfiguration(storage);
            var first = CreateHandleContext(config, [1], false, "alice", totalSize: 2);
            first.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = FileUpload.GetFileUploadToken(first, null, 1);
            var middleware = DotvvmFileUploadMiddleware.TryCreate(first.Services)!;
            await middleware.Handle(first);
            var continuation = first.HttpContext.Response.Headers["X-DotVVM-UploadToken"]!;

            var differentUser = CreateHandleContext(config, [2], true, "bob");
            differentUser.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            Assert.IsTrue(await middleware.Handle(differentUser));
            StringAssert.StartsWith(differentUser.HttpContext.Response.ContentType, "text/plain");
            Assert.AreEqual(1, storage.StoredBytes.Length);
            Assert.IsTrue(string.IsNullOrEmpty(differentUser.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));

            var bytes = Convert.FromBase64String(continuation);
            bytes[bytes.Length - 1] ^= 1;
            var tampered = CreateHandleContext(config, [2], true, "alice");
            tampered.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = Convert.ToBase64String(bytes);
            Assert.IsTrue(await middleware.Handle(tampered));
            StringAssert.StartsWith(tampered.HttpContext.Response.ContentType, "text/plain");
            Assert.AreEqual(1, storage.StoredBytes.Length);
            Assert.IsTrue(string.IsNullOrEmpty(tampered.HttpContext.Response.Headers["X-DotVVM-UploadToken"]));

            var valid = CreateHandleContext(config, [2], true, "alice");
            valid.HttpContext.Request.Headers["X-DotVVM-UploadToken"] = continuation;
            await middleware.Handle(valid);
            using var result = ReadResponseJson(valid);
            Assert.AreEqual(1, result.RootElement.GetProperty("result").GetArrayLength());
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, storage.StoredBytes);
        }

        private static DotvvmConfiguration CreateProtectedUploadConfiguration(IUploadedFileStorage storage) =>
            DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton(storage);
                s.AddSingleton<IViewModelProtector>(new DefaultViewModelProtector(new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider()));
            });

        private static IDotvvmRequestContext CreateHandleContext(DotvvmConfiguration config, byte[] bytes, bool final, string user, long? totalSize = null)
        {
            var context = CreateChunkContext(bytes, final, configuration: config, totalSize: totalSize);
            var http = (TestHttpContext)context.HttpContext;
            http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user) }, "Test"));
            http.Request.Method = "POST";
            http.Request.Path = "/" + HostingConstants.FileUploadHandlerMatchUrl;
            http.Request.ContentType = "multipart/form-data; boundary=dotvvm-test-boundary";
            context.HttpContext.Request.Headers["X-DotVVM-CsrfToken"] = "Not a CSRF token.";
            return context;
        }

        private static JsonDocument ReadResponseJson(IDotvvmRequestContext context)
        {
            context.HttpContext.Response.Body.Position = 0;
            return JsonDocument.Parse(context.HttpContext.Response.Body);
        }
#endif

        private static DotvvmFileUploadMiddleware CreateMiddleware(IUploadedFileStorage storage) =>
            new(null!, storage, null!, null!, new DotvvmTestHelper.FakeProtector());

        private static async Task<List<UploadedFile>> SaveChunk(IDotvvmRequestContext context, IUploadedFileStorage storage, FileUploadToken token)
        {
            var result = new List<UploadedFile>();
            var method = typeof(DotvvmFileUploadMiddleware).GetMethod("SaveChunk", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await (Task)method.Invoke(CreateMiddleware(storage), new object[] { context, "dotvvm-test-boundary", result, token })!;
            return result;
        }

        private static IDotvvmRequestContext CreateChunkContext(byte[] bytes, bool final, int chunkSize = 3 * 1024 * 1024, bool extraSection = false, DotvvmConfiguration configuration = null, long? totalSize = null)
        {
            var config = configuration ?? DotvvmTestHelper.CreateConfiguration();
            config.Runtime.FileUploadChunkSizeBytes = chunkSize;
            var context = DotvvmTestHelper.CreateContext(config);
            context.HttpContext.Request.Headers["X-DotVVM-UploadChunk"] = "true";
            context.HttpContext.Request.Headers["X-DotVVM-UploadFinalChunk"] = final ? "true" : "false";
            if (totalSize is {} total)
                context.HttpContext.Request.Headers["X-DotVVM-UploadFileSize"] = total.ToString(CultureInfo.InvariantCulture);
            using var body = new MemoryStream();
            WriteAscii(body, "--dotvvm-test-boundary\r\nContent-Disposition: form-data; name=\"upload\"; filename=\"test.txt\"\r\nContent-Type: text/plain\r\n\r\n");
            body.Write(bytes, 0, bytes.Length);
            if (extraSection)
                WriteAscii(body, "\r\n--dotvvm-test-boundary\r\nContent-Disposition: form-data; name=\"extra\"\r\n\r\nextra");
            WriteAscii(body, "\r\n--dotvvm-test-boundary--\r\n");
            var requestBody = context.HttpContext.Request.Body;
            body.Position = 0;
            body.CopyTo(requestBody);
            requestBody.Position = 0;
            return context;
        }

        private sealed class TestChunkStorage : IChunkedUploadedFileStorage
        {
            public Guid Id { get; } = Guid.NewGuid();
            public byte[] StoredBytes { get; private set; } = [];
            public int StoreCalls { get; private set; }
            public async Task<Guid> StoreFileAsync(Stream stream)
            {
                StoreCalls++;
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer);
                StoredBytes = buffer.ToArray();
                return Id;
            }
            public async Task AppendFileAsync(Guid id, long expectedPosition, Stream stream)
            {
                if (id != Id || StoredBytes.LongLength != expectedPosition)
                    throw new InvalidOperationException("Invalid upload continuation position.");
                using var buffer = new MemoryStream();
                buffer.Write(StoredBytes, 0, StoredBytes.Length);
                await stream.CopyToAsync(buffer);
                StoredBytes = buffer.ToArray();
            }
            public Task DeleteFileAsync(Guid id) => Task.CompletedTask;
            public Task<Stream> GetFileAsync(Guid id) => Task.FromResult<Stream>(new MemoryStream(StoredBytes));
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
