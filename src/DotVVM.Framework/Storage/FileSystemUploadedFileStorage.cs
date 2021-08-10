using System;
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
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            AutoDeleteInterval = autoDeleteInterval;

            autoDeleteTimer = new Timer(state => DeleteOldFiles(DateTime.Now - AutoDeleteInterval), null, AutoDeleteInterval, AutoDeleteInterval);
        }


        /// <summary>
        /// Stores uploaded file and returns its unique id.
        /// </summary>
        public async Task<Guid> StoreFileAsync(Stream stream)
        {
            var id = SecureGuidGenerator.GenerateGuid();
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
            var files = Directory.GetFiles(TempDirectory).Where(t => File.GetCreationTime(t) < maxCreatedDate);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
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
