using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace DotVVM.Framework.Storage
{
    public class ReturnedFileMetadata
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public Dictionary<string, string[]> AdditionalHeaders { get; set; }
        public string AttachmentDispositionType { get; set; }
    }
}
