using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
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
        public void Write(ReadOnlyMemory<char> text)
        {
            OriginalResponse.Write(text.ToString());
        }
        public void Write(ReadOnlyMemory<byte> data)
        {
            if (MemoryMarshal.TryGetArray(data, out var array))
                OriginalResponse.Write(array.Array, array.Offset, array.Count);
            else
                OriginalResponse.Write(data.ToArray());
        }
        public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            if (MemoryMarshal.TryGetArray(data, out var array))
                return OriginalResponse.WriteAsync(array.Array, array.Offset, array.Count, token);
            else
                return OriginalResponse.WriteAsync(data.ToArray(), token);
        }

        public Task WriteAsync(ReadOnlyMemory<char> text, CancellationToken token = default)
        {
            return OriginalResponse.WriteAsync(text.ToString(), token);
        }

        public Task WriteAsync(string text, CancellationToken token = default)
        {
            return OriginalResponse.WriteAsync(text, token);
        }
    }
}
