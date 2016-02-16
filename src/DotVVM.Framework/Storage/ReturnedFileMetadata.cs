using Microsoft.Owin;

namespace DotVVM.Framework.Storage
{
    public class ReturnedFileMetadata
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public IHeaderDictionary AdditionalHeaders { get; set; }
    }
}