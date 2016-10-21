using System;
using System.IO;

namespace DotVVM.Framework.Storage
{
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