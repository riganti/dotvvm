using System.Collections.Generic;

namespace DotVVM.Framework.Storage
{
    public class ReturnedFileMetadata
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public Dictionary<string, string[]> AdditionalHeaders { get; set; }
    }
}