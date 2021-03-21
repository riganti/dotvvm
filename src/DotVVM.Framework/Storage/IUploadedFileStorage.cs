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
        Task<Guid> StoreFileAsync(Stream stream);

        /// <summary>
        /// Deletes the uploaded file with the specified ID.
        /// </summary>
        Task DeleteFileAsync(Guid fileId);

        /// <summary>
        /// Gets the stream of the file with the specified ID.
        /// </summary>
        Task<Stream> GetFileAsync(Guid fileId);

    }
}
