#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestHttpResponse : IHttpResponse
    {

        public TestHttpResponse(IHttpContext httpContext)
        {
            this.Context = httpContext;
        }

        public TestHeaderCollection Headers { get; } = new TestHeaderCollection();
        IHeaderCollection IHttpResponse.Headers => Headers;

        public IHttpContext Context { get; }

        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public MemoryStream Body { get; set; } = new MemoryStream();

        public TimeSpan AsyncWriteDelay { get; set; } = TimeSpan.FromMilliseconds(1);
        Stream IHttpResponse.Body
        {
            get => Body;
            set => throw new NotSupportedException();
        }

        public void Write(string text) => Write(Encoding.UTF8.GetBytes(text));

        public void Write(byte[] data) => Body.Write(data, 0, data.Length);

        public void Write(byte[] data, int offset, int count) => Body.Write(data, offset, count);

        public Task WriteAsync(string text) => WriteAsync(text, default);

        public async Task WriteAsync(string text, CancellationToken token)
        {
            if (AsyncWriteDelay > TimeSpan.Zero)
                await Task.Delay(AsyncWriteDelay, token);
            Write(text);
        }
    }
}
