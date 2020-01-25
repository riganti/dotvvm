#nullable enable
using System.Collections.Generic;
using DotVVM.Framework.Storage;

namespace DotVVM.Framework.Controls
{
    public class UploadedFilesCollection
    {
        public UploadedFilesCollection()
        {
            Files = new List<UploadedFile>();
        }

        public int Progress { get; set; }

        public bool IsBusy { get; set; }

        public List<UploadedFile> Files { get; set; }

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
