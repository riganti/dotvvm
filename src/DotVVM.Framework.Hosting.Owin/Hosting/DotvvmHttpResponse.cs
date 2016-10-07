using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpResponse : IHttpResponse
    {
        public IOwinResponse OriginalResponse { get; }
        public IHttpContext Context { get; }

        public DotvvmHttpResponse(IOwinResponse originalResponse, IHttpContext context, IHeaderCollection headers)
        {
            Context = context;
            OriginalResponse = originalResponse;
            Headers = headers;
        }

        public IHeaderCollection Headers { get; set; }

        public int StatusCode
        {
            get { return OriginalResponse.StatusCode; }
            set { OriginalResponse.StatusCode = value; }
        }

        public string ContentType
        {
            get { return OriginalResponse.ContentType; }
            set { OriginalResponse.ContentType = value; }
        }

        public Stream Body
        {
            get { return OriginalResponse.Body; }
            set { OriginalResponse.Body = value; }
        }

        public void Write(string text)
        {
            OriginalResponse.Write(text);
        }

        public void Write(byte[] data)
        {
            OriginalResponse.Write(data);
        }

        public void Write(byte[] data, int offset, int count)
        {
            OriginalResponse.Body.Write(data, offset, count);
        }

        public Task WriteAsync(string text)
        {
            return OriginalResponse.WriteAsync(text);
        }

        public Task WriteAsync(string text, CancellationToken token)
        {
            return OriginalResponse.WriteAsync(text, token);
        }
    }
}