using System.Collections.Generic;

namespace DotVVM.Core.Storage
{
    public class ReturnedFileMetadata
    {
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public Dictionary<string, string[]> AdditionalHeaders { get; set; } = new Dictionary<string, string[]>();
        public string? AttachmentDispositionType { get; set; }
    }
}
