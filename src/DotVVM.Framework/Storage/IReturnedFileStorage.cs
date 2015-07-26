using System;
using System.IO;
using Microsoft.Owin;

namespace DotVVM.Framework.Storage
{
    public interface IReturnedFileStorage
    {
        /// <summary>
        /// Generates new file ID for future usage.
        /// </summary>
        /// <returns>32-characters-long string.</returns>
        string GenerateFileId();

        /// <summary>
        /// Stores data and metadata under given ID.
        /// </summary>
        /// <param name="id">ID for data and metadata.</param>
        /// <param name="bytes">Array of bytes to be stored.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="additionalHeaders">Additional headers.</param>
        void StoreFile(string id, byte[] bytes, string fileName, string mimeType, IHeaderDictionary additionalHeaders);

        /// <summary>
        /// Stores data and metadata under given ID.
        /// </summary>
        /// <param name="id">ID for data and metadata.</param>
        /// <param name="stream">Stream of data to be stored.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="additionalHeaders">Additional headers.</param>
        void StoreFile(string id, Stream stream, string fileName, string mimeType, IHeaderDictionary additionalHeaders);

        /// <summary>
        /// Gets stored data and metadata.
        /// </summary>
        /// <param name="id">ID for daa and metadata.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="additionalHeaders">Additional headers.</param>
        /// <returns>Stream of stored data.</returns>
        Stream GetFile(string id, out string fileName, out string mimeType, out IHeaderDictionary additionalHeaders);

        /// <summary>
        /// Deletes data and metadata under given ID.
        /// </summary>
        /// <param name="id">ID of data and metadata.</param>
        void DeleteFile(string id);

        /// <summary>
        /// Deletes all old data and metadata.
        /// </summary>
        /// <param name="maxCreatedDate">All data and metadata crated before this date time are deleted.</param>
        void DeleteOldFiles(DateTime maxCreatedDate);
    }
}