using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Core.Storage
{
    public interface IReturnedFileStorage
    {
        /// <summary>
        /// Stores the file and returns its unique ID.
        /// </summary>
        Task<Guid> StoreFileAsync(Stream stream, ReturnedFileMetadata metadata);

        /// <summary>
        /// Gets the file from the storage.
        /// </summary>
        Task<ReturnedFile> GetFileAsync(Guid fileId);

        /// <summary>
        /// Deletes the file with the specified ID.
        /// </summary>
        Task DeleteFileAsync(Guid fileId);

    }
}
