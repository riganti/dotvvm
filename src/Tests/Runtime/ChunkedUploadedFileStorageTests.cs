using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ChunkedUploadedFileStorageTests
    {
        private string directory = null!;
        private FileSystemUploadedFileStorage storage = null!;

        [TestInitialize]
        public void Initialize()
        {
            directory = Path.Combine(Path.GetTempPath(), "chunk-storage-tests-" + Guid.NewGuid());
            storage = new FileSystemUploadedFileStorage(directory, TimeSpan.FromHours(1));
        }

        [TestCleanup]
        public void Cleanup()
        {
            storage.Dispose();
            Directory.Delete(directory, true);
        }

        [TestMethod]
        public async Task Append_RejectsReplayAndOutOfOrderWithoutChangingFile()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1, 2]));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            await storage.AppendFileAsync(id, 2, new MemoryStream([3, 4]));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => storage.AppendFileAsync(id, 2, new MemoryStream([5])));
            using var file = await storage.GetFileAsync(id);
            using var bytes = new MemoryStream();
            await file.CopyToAsync(bytes);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, bytes.ToArray());
        }

        [TestMethod]
        public async Task Append_ExclusiveLockPreventsConcurrentWrites()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            var blocked = new BlockingStream();
            var append = storage.AppendFileAsync(id, 1, blocked);
            try
            {
                await AwaitWithTimeout(blocked.Entered.Task);
                await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            }
            finally
            {
                blocked.Release.TrySetResult(true);
                await AwaitWithTimeout(append);
            }
            using var file = await storage.GetFileAsync(id);
            Assert.AreEqual(2L, file.Length);
            Assert.AreEqual(1, file.ReadByte());
            Assert.AreEqual(2, file.ReadByte());
        }

        [TestMethod]
        public async Task Append_ConcurrentTokenUseAcrossStorageInstancesHasExactlyOneWinner()
        {
            const int writerCount = 32;
            var writers = Enumerable.Range(0, writerCount)
                .Select(_ => new FileSystemUploadedFileStorage(directory, TimeSpan.FromHours(1)))
                .ToArray();
            try
            {
                for (var round = 0; round < 20; round++)
                {
                    var expected = CreatePayload(round, 0, writerCount);
                    var id = await storage.StoreFileAsync(new MemoryStream(expected));
                    for (var chunk = 1; chunk <= 3; chunk++)
                    {
                        var position = expected.LongLength;
                        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        var allAttempted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        var remaining = writerCount;
                        var payloads = Enumerable.Range(0, writerCount)
                            .Select(writer => CreatePayload(round, chunk, writer)).ToArray();
                        var attempts = Enumerable.Range(0, writerCount).Select(writer => Task.Run(async () =>
                        {
                            var reported = 0;
                            void ReportAttempt()
                            {
                                if (Interlocked.Exchange(ref reported, 1) == 0 &&
                                    Interlocked.Decrement(ref remaining) == 0)
                                    allAttempted.TrySetResult(true);
                            }
                            await start.Task;
                            try
                            {
                                using var source = new ContendingStream(payloads[writer], ReportAttempt, release.Task);
                                await writers[writer].AppendFileAsync(id, position, source);
                                return (Exception)null;
                            }
                            catch (Exception exception)
                            {
                                return exception;
                            }
                            finally
                            {
                                ReportAttempt();
                            }
                        })).ToArray();
                        var finished = Task.WhenAll(attempts);
                        try
                        {
                            start.TrySetResult(true);
                            // Keep every accepted writer before its first byte until every contender
                            // has either acquired the file or been rejected. Position checks alone
                            // cannot serialize writers in this window.
                            await AwaitWithTimeout(allAttempted.Task);
                        }
                        finally
                        {
                            start.TrySetResult(true);
                            release.TrySetResult(true);
                            await AwaitWithTimeout(finished);
                        }

                        var results = await finished;
                        var context = $"Round {round}, chunk {chunk}";
                        Assert.AreEqual(1, results.Count(result => result == null), context);
                        for (var writer = 0; writer < writerCount; writer++)
                        {
                            Assert.IsTrue(results[writer] == null ||
                                results[writer] is IOException ||
                                results[writer] is InvalidOperationException,
                                $"{context}, writer {writer}: {results[writer]}");
                        }
                        var winner = Array.FindIndex(results, result => result == null);
                        var previous = expected;
                        expected = previous.Concat(payloads[winner]).ToArray();
                        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                            writers[(winner + 1) % writerCount].AppendFileAsync(id, position, new MemoryStream(payloads[winner])));
                        using (var file = await storage.GetFileAsync(id))
                        using (var actual = new MemoryStream())
                        {
                            await file.CopyToAsync(actual);
                            var bytes = actual.ToArray();
                            Assert.AreEqual(expected.LongLength, file.Length, context);
                            CollectionAssert.AreEqual(previous, bytes.Take(previous.Length).ToArray(), context + ": prefix");
                            CollectionAssert.AreEqual(payloads[winner], bytes.Skip(previous.Length).ToArray(), context + ": winning chunk");
                            CollectionAssert.AreEqual(expected, bytes, context);
                        }
                    }
                    await storage.DeleteFileAsync(id);
                }
            }
            finally
            {
                foreach (var writer in writers)
                    writer.Dispose();
            }
        }

#if DotNetCore
        [TestMethod]
        public async Task Append_ByteRangeLockRejectsOtherProcessesEvenAfterReaderCloses()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            var blocked = new BlockingStream();
            var append = storage.AppendFileAsync(id, 1, blocked);
            try
            {
                await AwaitWithTimeout(blocked.Entered.Task);
                // Closing a reader of the data inode must not release the append lock on Unix.
                using (var reader = new FileStream(
                    Directory.GetFiles(directory).Single(path => path.EndsWith(".tmp", StringComparison.Ordinal)),
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Assert.AreEqual(1, reader.ReadByte());
                }
                await Task.WhenAll(
                    RunLockWorker(id, "first"),
                    RunLockWorker(id, "second"));
            }
            finally
            {
                blocked.Release.TrySetResult(true);
                await AwaitWithTimeout(append);
            }
            using var file = await storage.GetFileAsync(id);
            using var bytes = new MemoryStream();
            await file.CopyToAsync(bytes);
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, bytes.ToArray());
        }

        [TestMethod]
        public async Task Append_CrossProcessWorker()
        {
            var sharedDirectory = Environment.GetEnvironmentVariable("DOTVVM_CHUNK_LOCK_DIRECTORY");
            if (string.IsNullOrEmpty(sharedDirectory))
                return;
            var id = Guid.Parse(Environment.GetEnvironmentVariable("DOTVVM_CHUNK_LOCK_ID"));
            var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempts = Enumerable.Range(0, 32).Select(writer => Task.Run(async () =>
            {
                using var other = new FileSystemUploadedFileStorage(sharedDirectory, TimeSpan.FromHours(1));
                using var source = new MemoryStream(CreatePayload(0, 1, writer));
                await start.Task;
                await Assert.ThrowsExceptionAsync<IOException>(() => other.AppendFileAsync(id, 1, source));
            })).ToArray();
            start.TrySetResult(true);
            await AwaitWithTimeout(Task.WhenAll(attempts));
            File.WriteAllText(Environment.GetEnvironmentVariable("DOTVVM_CHUNK_LOCK_WITNESS"), "completed");
        }

        private async Task RunLockWorker(Guid id, string name)
        {
            var witness = Path.Combine(directory, name + ".completed");
            var start = new ProcessStartInfo("dotnet")
            {
                Arguments = $"vstest \"{typeof(ChunkedUploadedFileStorageTests).Assembly.Location}\" --Tests:{nameof(Append_CrossProcessWorker)} --logger:console;verbosity=minimal",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            start.EnvironmentVariables["DOTVVM_CHUNK_LOCK_DIRECTORY"] = directory;
            start.EnvironmentVariables["DOTVVM_CHUNK_LOCK_ID"] = id.ToString();
            start.EnvironmentVariables["DOTVVM_CHUNK_LOCK_WITNESS"] = witness;
            using var child = new Process { StartInfo = start };
            Assert.IsTrue(child.Start(), "Could not start the cross-process lock worker.");
            var output = child.StandardOutput.ReadToEndAsync();
            var error = child.StandardError.ReadToEndAsync();
            try
            {
                await AwaitWithTimeout(child.WaitForExitAsync());
                await AwaitWithTimeout(Task.WhenAll(output, error));
                Assert.AreEqual(0, child.ExitCode, await output + Environment.NewLine + await error);
                Assert.IsTrue(File.Exists(witness), "The selected child worker did not complete.");
            }
            finally
            {
                if (!child.HasExited)
                {
                    child.Kill(entireProcessTree: true);
                    await AwaitWithTimeout(child.WaitForExitAsync());
                }
                File.Delete(witness);
            }
        }
#endif

        [TestMethod]
        public async Task DeleteFile_RemovesDataAndSidecar()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await storage.AppendFileAsync(id, 1, new MemoryStream([2]));
            Assert.AreEqual(2, Directory.GetFiles(directory).Length);
            await storage.DeleteFileAsync(id);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task DeleteOldFiles_RemovesDataAndSidecar()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await storage.AppendFileAsync(id, 1, new MemoryStream([2]));
            Assert.AreEqual(2, Directory.GetFiles(directory).Length);
            storage.DeleteOldFiles(DateTime.Now.AddMinutes(1));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task DeleteOldFiles_RemovesOrphanSidecar()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await storage.AppendFileAsync(id, 1, new MemoryStream([2]));
            var dataFile = Directory.GetFiles(directory).Single(path => !path.EndsWith(".lock", StringComparison.Ordinal));
            File.Delete(dataFile);
            storage.DeleteOldFiles(DateTime.Now.AddMinutes(1));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task DeleteOldFiles_PreservesOlderSidecarWhileDataExists()
        {
            var dataFile = Path.Combine(directory, $"dotvvm-uploaded-file-{Guid.NewGuid()}.tmp");
            var lockFile = dataFile + ".lock";
            File.WriteAllBytes(lockFile, new byte[0]);
            var cutoff = File.GetCreationTime(lockFile).AddTicks(1);
            // Only the sidecar should qualify for deletion, including on coarse-resolution filesystems.
            for (var attempt = 0; attempt < 120; attempt++)
            {
                File.WriteAllBytes(dataFile, new byte[] { 1 });
                if (File.GetCreationTime(dataFile) >= cutoff)
                    break;
                File.Delete(dataFile);
                await Task.Delay(25);
            }
            Assert.IsTrue(File.Exists(dataFile), "Could not create data newer than its sidecar.");
            storage.DeleteOldFiles(cutoff);
            Assert.IsTrue(File.Exists(dataFile));
            Assert.IsTrue(File.Exists(lockFile), "Cleanup must not unlink a live file's lock.");
        }

        [TestMethod]
        public async Task Append_MissingFileDoesNotCreateSidecar()
        {
            var id = Guid.NewGuid();
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() =>
                storage.AppendFileAsync(id, 1, new MemoryStream([2])));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() =>
                storage.AppendFileAsync(id, 1, new MemoryStream([2])));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [DataTestMethod]
        [DataRow(0L)]
        [DataRow(-1L)]
        public async Task Append_InvalidPositionDoesNotDeleteLegitimateFile(long position)
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1, 2]));
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => storage.AppendFileAsync(id, position, new MemoryStream([3])));
            using var file = await storage.GetFileAsync(id);
            Assert.AreEqual(2L, file.Length);
            Assert.AreEqual(1, file.ReadByte());
            Assert.AreEqual(2, file.ReadByte());
        }

        [TestMethod]
        public async Task Append_ConcurrentRejectedAttemptDoesNotInterfereWithOwnedFailureCleanup()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            var blocked = new BlockingStream { FailAfterWrite = true };
            var append = storage.AppendFileAsync(id, 1, blocked);
            try
            {
                await AwaitWithTimeout(blocked.Entered.Task);
                await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            }
            finally
            {
                blocked.Release.TrySetResult(true);
                await Assert.ThrowsExceptionAsync<IOException>(() => AwaitWithTimeout(append));
            }
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.GetFileAsync(id));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task Append_ReadFailureInvalidatesPartialFile()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new FailingStream()));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.GetFileAsync(id));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task Store_ReadFailureRemovesPartialFile()
        {
            await Assert.ThrowsExceptionAsync<IOException>(() => storage.StoreFileAsync(new FailingStream()));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        [TestMethod]
        public async Task Append_EmptyChunkCannotLeaveReplayablePosition()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => storage.AppendFileAsync(id, 1, new MemoryStream()));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.GetFileAsync(id));
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }

        private sealed class FailingStream : MemoryStream
        {
            public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                await destination.WriteAsync(new byte[] { 2 }, 0, 1, cancellationToken);
                throw new IOException("Simulated interrupted upload.");
            }
        }

        private static byte[] CreatePayload(int round, int chunk, int writer)
        {
            var bytes = new byte[8192 + writer * 37 + chunk * 101];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(round * 17 + chunk * 31 + writer * 43 + i * 13);
            Buffer.BlockCopy(BitConverter.GetBytes(round), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(chunk), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(writer), 0, bytes, 8, 4);
            return bytes;
        }

        private static async Task AwaitWithTimeout(Task task)
        {
            using var timeout = new CancellationTokenSource();
            var delay = Task.Delay(TimeSpan.FromSeconds(30), timeout.Token);
            Assert.AreSame(task, await Task.WhenAny(task, delay), "Concurrent upload attempts timed out.");
            timeout.Cancel();
            await task;
        }

        private sealed class ContendingStream : MemoryStream
        {
            private readonly byte[] payload;
            private readonly Action reportAttempt;
            private readonly Task release;

            public ContendingStream(byte[] payload, Action reportAttempt, Task release)
            {
                this.payload = payload;
                this.reportAttempt = reportAttempt;
                this.release = release;
            }

            public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                reportAttempt();
                await AwaitWithTimeout(release);
                for (var offset = 0; offset < payload.Length; offset += 1024)
                {
                    await Task.Yield();
                    await destination.WriteAsync(payload, offset, Math.Min(1024, payload.Length - offset), cancellationToken);
                }
            }
        }

        private sealed class BlockingStream : MemoryStream
        {
            public bool FailAfterWrite { get; set; }
            public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                Entered.TrySetResult(true);
                await Release.Task;
                await destination.WriteAsync(new byte[] { 2 }, 0, 1, cancellationToken);
                if (FailAfterWrite)
                    throw new IOException("Simulated interrupted upload.");
            }
        }
    }
}
