using System.Collections.Generic;
using DotVVM.Core.Storage;

namespace DotVVM.Framework.Controls
{
    /// <summary> A view model for the FileUpload control. </summary>
    public class UploadedFilesCollection
    {
        public UploadedFilesCollection()
        {
            Files = new List<UploadedFile>();
        }

        /// <summary> if <see cref="IsBusy"/> is true, this property contains the upload progress in percents (0-100). </summary>
        public int Progress { get; set; }

        /// <summary> Indicates whether something is being uploaded at the moment. </summary>
        public bool IsBusy { get; set; }

        /// <summary> List of all completely uploaded files. </summary>
        public List<UploadedFile> Files { get; set; }

        /// <summary> Contains an error message indicating if there was a problem during the upload. </summary>
        public string? Error { get; set; }

        public void Clear()
        {
            Progress = 0;
            IsBusy = false;
            Files.Clear();
            Error = string.Empty;
        }
    }
}
