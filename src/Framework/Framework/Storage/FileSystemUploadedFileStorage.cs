using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Storage
{
    public class FileSystemUploadedFileStorage : IChunkedUploadedFileStorage, IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, byte> activeAppends = new ConcurrentDictionary<Guid, byte>();

        public string TempDirectory { get; private set; }

        public TimeSpan AutoDeleteInterval { get; private set; }


        private Timer autoDeleteTimer;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemUploadedFileStorage"/> class.
        /// </summary>
        public FileSystemUploadedFileStorage(string tempDirectory, TimeSpan autoDeleteInterval)
        {
            if (string.IsNullOrEmpty(tempDirectory))
            {
                throw new ArgumentNullException(nameof(tempDirectory));
            }
            
            TempDirectory = tempDirectory;
            AutoDeleteInterval = autoDeleteInterval;

            EnsureDirectory();

            autoDeleteTimer = new Timer(state => DeleteOldFiles(DateTime.Now - AutoDeleteInterval), null, AutoDeleteInterval, AutoDeleteInterval);
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(TempDirectory))
            {
                try
                {
                    Directory.CreateDirectory(TempDirectory);
                }
                catch (IOException e)
                {
                    throw new Exception($"The {nameof(FileSystemUploadedFileStorage)} couldn't create a directory {TempDirectory}. Make sure the application has write permissions for this path.", e);
                }
            }
        }

        /// <summary>
        /// Stores uploaded file and returns its unique id.
        /// </summary>
        public async Task<Guid> StoreFileAsync(Stream stream)
        {
            var id = SecureGuidGenerator.GenerateGuid();
            EnsureDirectory();
            using (var fs = new FileStream(GetFileName(id), FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                try
                {
                    await stream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
                catch
                {
                    fs.SetLength(0);
                    fs.Dispose();
                    await DeleteFileAsync(id);
                    throw;
                }
            }
            return id;
        }

        /// <summary>
        /// Appends a chunk while exclusively locking the file's position check and write.
        /// </summary>
        /// <remarks>
        /// Shared storage must support cross-client byte-range locks (for example, SMB or NFS with
        /// server-coordinated locking and close-to-open consistency enabled). Local-only or disabled
        /// network locks are not supported. Lock acquisition errors do not modify the uploaded data.
        /// </remarks>
        public async Task AppendFileAsync(Guid fileId, long expectedPosition, Stream stream)
        {
            if (expectedPosition <= 0)
                throw new ArgumentOutOfRangeException(nameof(expectedPosition));
            // Unix record locks are process-associated. Reject local contenders before opening
            // another handle, whose disposal could otherwise release the active writer's lock.
            if (!activeAppends.TryAdd(fileId, 0))
                throw new IOException("Another upload chunk is being appended to this file.");
            try
            {
                var fileName = GetFileName(fileId);
                if (!File.Exists(fileName))
                    throw new FileNotFoundException("The uploaded file does not exist.", fileName);
                // Do not combine an exclusive share lock with a record lock: they can conflict on NFS.
                // Use a separate inode so closing a GetFileAsync reader cannot release a Unix record lock.
                using (var appendLock = new FileStream(fileName + ".lock", FileMode.OpenOrCreate,
                    FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
                {
                    // Fail closed if locking is unsupported. Open and close data while locked
                    // so NFS close-to-open consistency also covers its length check and writeback.
                    appendLock.Lock(0, 1);
                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    {
                        if (fs.Length != expectedPosition)
                            throw new InvalidOperationException("Invalid upload continuation position.");
                        fs.Position = expectedPosition;
                        try
                        {
                            await stream.CopyToAsync(fs);
                            if (fs.Position == expectedPosition)
                                throw new InvalidOperationException("Upload continuation must not be empty.");
                            await fs.FlushAsync();
                        }
                        catch
                        {
                            // Poison the position before releasing the exclusive lock, even if deletion fails.
                            fs.SetLength(0);
                            fs.Dispose();
                            await DeleteFileAsync(fileId);
                            throw;
                        }
                    }
                }
            }
            finally
            {
                activeAppends.TryRemove(fileId, out _);
            }
        }

        /// <summary>
        /// Deletes the uploaded file.
        /// </summary>
        public Task DeleteFileAsync(Guid fileId)
        {
            try
            {
                File.Delete(GetFileName(fileId));
                File.Delete(GetFileName(fileId) + ".lock");
            }
            catch (IOException)
            {
            }
            return TaskUtils.GetCompletedTask();
        }

        /// <summary>
        /// Gets the file with the specified id.
        /// </summary>
        public Task<Stream> GetFileAsync(Guid fileId)
        {
            var stream = new FileStream(GetFileName(fileId), FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream>(stream);
        }

        /// <summary>
        /// Deletes files older than the specified date.
        /// </summary>
        public void DeleteOldFiles(DateTime maxCreatedDate)
        {
            List<string> files;
            try
            {
                files = Directory.GetFiles(TempDirectory)
                    .Where(t => File.GetCreationTime(t) < maxCreatedDate)
                    .Where(t => Path.GetFileName(t).StartsWith("dotvvm-uploaded-file-", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (IOException)
            {
                // the directory probably doesn't exist, we don't have to delete anything
                return;
            }

            foreach (var file in files)
            {
                try
                {
                    // Never unlink the lock for an existing file: another writer could recreate it.
                    if (file.EndsWith(".lock", StringComparison.OrdinalIgnoreCase) && File.Exists(file.Substring(0, file.Length - 5)))
                        continue;
                    File.Delete(file);
                    if (!file.EndsWith(".lock", StringComparison.OrdinalIgnoreCase))
                        File.Delete(file + ".lock");
                }
                catch (IOException)
                {
                    // some of the files couldn't be deleted, it is probably locked
                }
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        private string GetFileName(Guid id)
        {
            return Path.Combine(TempDirectory, $"dotvvm-uploaded-file-{id}.tmp");
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (autoDeleteTimer != null)
            {
                autoDeleteTimer.Dispose();
            }
        }
    }
}
