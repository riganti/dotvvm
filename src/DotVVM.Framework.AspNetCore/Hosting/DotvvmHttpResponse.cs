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

        public DotvvmHttpResponse(HttpResponse originalResponse)
        {
            OriginalResponse = originalResponse;
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
            throw new System.NotImplementedException();
        }

        public void Write(byte[] data)
        {
            OriginalResponse.Write(data);
        }

        public void Write(byte[] data, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync(string text)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync(string text, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}