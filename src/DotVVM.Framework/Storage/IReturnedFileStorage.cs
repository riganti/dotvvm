using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Storage
{
    public interface IReturnedFileStorage
    {
        /// <summary>
        /// Stores the file and returns its unique ID.
        /// </summary>
        [Obsolete("Don't bother with implementing this one, it is not used anymore")]
        Task<Guid> StoreFile(byte[] bytes, ReturnedFileMetadata metadata);

        /// <summary>
        /// Stores the file and returns its unique ID.
        /// </summary>
        Task<Guid> StoreFile(Stream stream, ReturnedFileMetadata metadata);

        /// <summary>
        /// Gets the file from the storage.
        /// </summary>
        Stream GetFile(Guid fileId, out ReturnedFileMetadata metadata);

        /// <summary>
        /// Deletes the file with the specified ID.
        /// </summary>
        void DeleteFile(Guid fileId);

        /// <summary>
        /// Deletes all files older than the specified date.
        /// </summary>
        void DeleteOldFiles(DateTime maxCreatedDate);
    }
}