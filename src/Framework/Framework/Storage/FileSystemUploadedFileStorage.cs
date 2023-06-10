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
    public class FileSystemUploadedFileStorage : IUploadedFileStorage, IDisposable
    {
        public string TempDirectory { get; private set; }

        public TimeSpan AutoDeleteInterval { get; private set; }


        private Timer autoDeleteTimer;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemUploadedFileStorage"/> class.
        /// </summary>
        public FileSystemUploadedFileStorage(string tempDirectory, TimeSpan autoDeleteInterval)
        {
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
                catch (IOException)
                {
                    throw new Exception($"The {nameof(FileSystemUploadedFileStorage)} couldn't create a directory {TempDirectory}. Make sure the application has write permissions for this path.");
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
            using (var fs = new FileStream(GetFileName(id), FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
            return id;
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
            return Path.Combine(TempDirectory, id + ".tmp");
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
