using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
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
            // ASP.NET Core does not support synchronous writes, so we use GetResult()
            OriginalResponse.WriteAsync(text).GetAwaiter().GetResult();
        }
        public void Write(ReadOnlyMemory<char> text)
        {
            this.WriteAsync(text).GetAwaiter().GetResult();
        }

        public void Write(ReadOnlyMemory<byte> data)
        {
            OriginalResponse.Body.WriteAsync(data).GetAwaiter().GetResult();
        }

        public Task WriteAsync(string text, CancellationToken token = default)
        {
            return OriginalResponse.WriteAsync(text, token);
        }
        public Task WriteAsync(ReadOnlyMemory<char> text, CancellationToken token = default)
        {
            var writer = new StreamWriter(OriginalResponse.Body, StringUtils.Utf8) { AutoFlush = true };
            return writer.WriteAsync(text, token);
        }

        public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            var task = OriginalResponse.Body.WriteAsync(data, token);
            return task.IsCompletedSuccessfully ? Task.CompletedTask : task.AsTask();
        }
    }
}
