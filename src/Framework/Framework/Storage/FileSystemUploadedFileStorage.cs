using System;
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

        public async Task AppendFileAsync(Guid fileId, long expectedPosition, Stream stream)
        {
            if (expectedPosition <= 0)
                throw new ArgumentOutOfRangeException(nameof(expectedPosition));
            using (var fs = new FileStream(GetFileName(fileId), FileMode.Open, FileAccess.Write, FileShare.None))
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

        /// <summary>
        /// Deletes the uploaded file.
        /// </summary>
        public Task DeleteFileAsync(Guid fileId)
        {
            try
            {
                File.Delete(GetFileName(fileId));
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
                    File.Delete(file);
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
