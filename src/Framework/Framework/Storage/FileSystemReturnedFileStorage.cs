using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.Storage
{
    public class FileSystemReturnedFileStorage : IReturnedFileStorage, IDisposable
    {

        /// <summary>
        /// Temp directory where are files stored.
        /// </summary>
        public string TempDirectory { get; private set; }

        /// <summary>
        /// Interval in which will be old files deleted.
        /// </summary>
        public TimeSpan AutoDeleteInterval { get; private set; }

        /// <summary>
        /// Timer for deleting old files.
        /// </summary>
        private readonly Timer _autoDeleteTimer;


        /// <summary>
        /// Initializes new instance of <see cref="FileSystemReturnedFileStorage"/> class with default interval for deleting old files.
        /// </summary>
        /// <param name="directory">Temp directory for storing files.</param>
        public FileSystemReturnedFileStorage(string directory)
            : this(directory, new TimeSpan(0, 5, 0))
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="FileSystemReturnedFileStorage"/> class.
        /// </summary>
        /// <param name="directory">Temp directory for storing files.</param>
        /// <param name="autoDeleteInterval">Interval for deleting old files.</param>
        public FileSystemReturnedFileStorage(string directory, TimeSpan autoDeleteInterval)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            TempDirectory = directory;
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            AutoDeleteInterval = autoDeleteInterval;

            _autoDeleteTimer = new Timer(state => DeleteOldFiles(DateTime.Now - AutoDeleteInterval), null, AutoDeleteInterval, AutoDeleteInterval);
        }

        private Guid GenerateFileId()
        {
            return SecureGuidGenerator.GenerateGuid();
        }

        public async Task<Guid> StoreFileAsync(Stream stream, ReturnedFileMetadata metadata)
        {
            var id = GenerateFileId();
            var dataFilePath = GetDataFilePath(id);
            using (var fs = new FileStream(dataFilePath, FileMode.Create))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }

            await StoreMetadata(id, metadata);
            return id;
        }

        private async Task StoreMetadata(Guid id, ReturnedFileMetadata metadata)
        {
            var metadataFilePath = GetMetadataFilePath(id);
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
#if DotNetCore 
            await File.WriteAllTextAsync(
#else
            File.WriteAllText(
#endif
                metadataFilePath, JsonConvert.SerializeObject(metadata, settings), Encoding.UTF8);
        }

        private string GetDataFilePath(Guid id)
        {
            return Path.Combine(TempDirectory, id + ".data");
        }

        private string GetMetadataFilePath(Guid id)
        {
            return Path.Combine(TempDirectory, id + ".metadata");
        }

        public Task<ReturnedFile> GetFileAsync(Guid id)
        {
            var metadataJson = File.ReadAllText(GetMetadataFilePath(id), Encoding.UTF8);
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;

            var stream = new FileStream(GetDataFilePath(id), FileMode.Open);
            var metadata = JsonConvert.DeserializeObject<ReturnedFileMetadata>(metadataJson, settings);

            return Task.FromResult(new ReturnedFile(stream, metadata));
        }

        public Task DeleteFileAsync(Guid id)
        {
            try
            {
                File.Delete(GetDataFilePath(id));
            }
            catch (IOException)
            {
            }

            try
            {
                File.Delete(GetMetadataFilePath(id));
            }
            catch (IOException)
            {
            }

            return TaskUtils.GetCompletedTask();
        }

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

        public void Dispose()
        {
            _autoDeleteTimer?.Dispose();
        }
    }
}
