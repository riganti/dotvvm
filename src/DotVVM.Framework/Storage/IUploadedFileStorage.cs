using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Storage
{
    public interface IUploadedFileStorage
    {

        /// <summary>
        /// Stores uploaded file and returns its unique id.
        /// </summary>
        Task<Guid> StoreFile(Stream stream);

        /// <summary>
        /// Deletes the uploaded file.
        /// </summary>
        void DeleteFile(Guid fileId);

        /// <summary>
        /// Gets the file with the specified id.
        /// </summary>
        Stream GetFile(Guid fileId);

        /// <summary>
        /// Deletes files older than the specified date.
        /// </summary>
        void DeleteOldFiles(DateTime maxCreatedDate);

    }
}
