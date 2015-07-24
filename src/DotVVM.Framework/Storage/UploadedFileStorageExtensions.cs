using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Storage
{
    public static class UploadedFileStorageExtensions
    {

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
