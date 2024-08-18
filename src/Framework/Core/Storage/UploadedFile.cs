using System;

namespace DotVVM.Core.Storage
{
    public class UploadedFile
    {
        /// <summary> A unique, randomly generated ID of the uploaded file. Use this ID to get the file from <see cref="IUploadedFileStorage.GetFileAsync(Guid)" /> </summary>
        public Guid FileId { get; set; }

        /// <summary> A user-specified name of the file. Use with caution, the user may specify this to be any string (for example <c>../../Web.config</c>). </summary>
        public string? FileName { get; set; }

        /// <summary> Length of the file in bytes. Use with caution, the user may manipulate with this property and it might not correspond to the file returned from <see cref="IUploadedFileStorage" />. </summary>
        public FileSize FileSize { get; set; } = new FileSize();

        /// <summary> If the file type matched one of type MIME types or extensions in <c>FileUpload.AllowedFileTypes</c>. Use with caution, the user may manipulate with this property. </summary>
        public bool IsFileTypeAllowed { get; set; } = true;

        /// <summary> If the file size is larger that the limit specified in <c>FileUpload.MaxFileSize</c>. Use with caution, the user may manipulate with this property. </summary>
        public bool IsMaxSizeExceeded { get; set; } = false;

        /// <summary> If the file satisfies both allowed file types and the size limit. Use with caution, the user may manipulate with this property. </summary>
        public bool IsAllowed
            => IsFileTypeAllowed && !IsMaxSizeExceeded;
    }
}
