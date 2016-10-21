using System;

namespace DotVVM.Framework.Storage
{
    public class UploadedFile
    {
        public Guid FileId { get; set; }

        public string FileName { get; set; }

        public bool FileTypeAllowed { get; set; } = true;

        public bool MaxSizeExceeded { get; set; } = false;

        public bool Allowed
            => FileTypeAllowed && !MaxSizeExceeded;
    }
}