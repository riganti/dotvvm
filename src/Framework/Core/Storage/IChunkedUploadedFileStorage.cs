using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Core.Storage
{
    /// <summary>Optional support for securely continuing an uploaded file.</summary>
    public interface IChunkedUploadedFileStorage : IUploadedFileStorage
    {
        /// <summary>
        /// Appends a nonempty chunk only if the file's length equals expectedPosition.
        /// The position check and append must be exclusive and atomic with respect to other uploads.
        /// A failed write must invalidate the partial file; rejected positions must not modify it.
        /// </summary>
        Task AppendFileAsync(Guid fileId, long expectedPosition, Stream stream);
    }
}
