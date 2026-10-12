using System;
using System.IO;
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
            await blocked.Entered.Task;
            try
            {
                await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            }
            finally
            {
                blocked.Release.TrySetResult(true);
                await append;
            }
            using var file = await storage.GetFileAsync(id);
            Assert.AreEqual(2L, file.Length);
            Assert.AreEqual(1, file.ReadByte());
            Assert.AreEqual(2, file.ReadByte());
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
            await blocked.Entered.Task;
            try
            {
                await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
            }
            finally
            {
                blocked.Release.TrySetResult(true);
                await Assert.ThrowsExceptionAsync<IOException>(() => append);
            }
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.GetFileAsync(id));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
        }

        [TestMethod]
        public async Task Append_ReadFailureInvalidatesPartialFile()
        {
            var id = await storage.StoreFileAsync(new MemoryStream([1]));
            await Assert.ThrowsExceptionAsync<IOException>(() => storage.AppendFileAsync(id, 1, new FailingStream()));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.GetFileAsync(id));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => storage.AppendFileAsync(id, 1, new MemoryStream([3])));
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
        }

        private sealed class FailingStream : MemoryStream
        {
            public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                await destination.WriteAsync(new byte[] { 2 }, 0, 1, cancellationToken);
                throw new IOException("Simulated interrupted upload.");
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
