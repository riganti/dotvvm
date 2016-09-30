using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Storage
{
    public interface IUploadedFileStorage
    {
        /// <summary>
        /// Stores uploaded file and returns its unique ID.
        /// </summary>
        Task<Guid> StoreFile(Stream stream);

        /// <summary>
        /// Deletes the uploaded file.
        /// </summary>
        void DeleteFile(Guid fileId);

        /// <summary>
        /// Gets the file with the specified ID.
        /// </summary>
        Stream GetFile(Guid fileId);

        /// <summary>
        /// Deletes files older than the specified date.
        /// </summary>
        void DeleteOldFiles(DateTime maxCreatedDate);
    }

    public static class UploadedFileStorageExtensions
    {
        /// <summary>
        /// Saves an uploaded file with the specified ID to the given location.
        /// </summary>
        public static void SaveAs(this IUploadedFileStorage storage, Guid fileId, string path)
        {
            using (var stream = storage.GetFile(fileId))
            {
                using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }
            }
        }
    }
}