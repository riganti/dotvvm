using System;

namespace DotVVM.Core.Storage
{
    public class UploadedFile
    {
        public Guid FileId { get; set; }

        public string? FileName { get; set; }

        public FileSize FileSize { get; set; } = new FileSize();

        public bool IsFileTypeAllowed { get; set; } = true;

        public bool IsMaxSizeExceeded { get; set; } = false;

        public bool IsAllowed
            => IsFileTypeAllowed && !IsMaxSizeExceeded;
    }
}
