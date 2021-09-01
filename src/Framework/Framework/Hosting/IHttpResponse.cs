using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpResponse
    {
        IHeaderCollection Headers { get; }
        IHttpContext Context { get; }
        int StatusCode { get; set; }
        string? ContentType { get; set; }
        Stream Body { get; set; }
        void Write(string text);
        void Write(byte[] data);
        void Write(byte[] data, int offset, int count);
        Task WriteAsync(string text);
        Task WriteAsync(string text, CancellationToken token);
    }
}
