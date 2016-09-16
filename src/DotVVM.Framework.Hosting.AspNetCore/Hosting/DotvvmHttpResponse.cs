using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpResponse : IHttpResponse
    {
        public HttpResponse OriginalResponse { get; }
        public IHttpContext Context { get; }

        public DotvvmHttpResponse(HttpResponse originalResponse, IHttpContext context, IHeaderCollection headers)
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
            var writer = new StreamWriter(OriginalResponse.Body) { AutoFlush = true};
            writer.Write(text);
        }

        public void Write(byte[] data)
        {
            OriginalResponse.Body.Write(data, 0, data.Length);
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