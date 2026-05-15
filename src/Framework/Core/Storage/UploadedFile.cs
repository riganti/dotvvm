using System;

namespace DotVVM.Core.Storage
{
    public class UploadedFile
    {
        public Guid FileId { get; set; }

        public string? FileName { get; set; }

        public FileSize FileSize { get; set; } = new FileSize();

        public bool IsFileTypeAllowed { get; set; } = true;

        /// <summary>
        /// If the file size is larger that the limit specified in <c>FileUpload.MaxFileSize</c>.
        /// Use with caution, the user may manipulate with this property.
        /// Note the with the default file upload backend, files exceeding the limit will be rejected immediately and won't be stored in this collection.
        /// </summary>
        public bool IsMaxSizeExceeded { get; set; } = false;

        public bool IsAllowed
            => IsFileTypeAllowed && !IsMaxSizeExceeded;
    }
}
