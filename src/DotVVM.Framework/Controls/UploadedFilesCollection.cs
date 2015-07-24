using System.Collections.Generic;
using Newtonsoft.Json;
using DotVVM.Framework.Storage;

namespace DotVVM.Framework.Controls
{
    public class UploadedFilesCollection
    {

        public List<UploadedFile> Files { get; set; }

        public string Error { get; set; }

        public int Progress { get; set; }
        
        public bool IsBusy { get; set; }


        public UploadedFilesCollection()
        {
            Files = new List<UploadedFile>();
        }

        public void Clear()
        {
            Files.Clear();
            Error = string.Empty;
            Progress = 0;
            IsBusy = false;
        }
    }
}