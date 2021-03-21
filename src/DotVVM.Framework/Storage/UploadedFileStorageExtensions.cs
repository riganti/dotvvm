using System;
using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Storage
{
    public static class UploadedFileStorageExtensions
    {
        /// <summary>
        /// Saves an uploaded file with the specified ID to the given location.
        /// </summary>
        public static async Task SaveAsAsync(this IUploadedFileStorage storage, Guid fileId, string path)
        {
            using (var stream = await storage.GetFileAsync(fileId))
            {
                using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }
        }
    }
}
