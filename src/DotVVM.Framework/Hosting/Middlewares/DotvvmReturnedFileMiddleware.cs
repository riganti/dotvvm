using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Storage;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
#if DotNetCore
using Microsoft.Net.Http.Headers;
#else
using System.Net.Http.Headers;
#endif

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmReturnedFileMiddleware : IMiddleware
    {
        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            if (url.StartsWith("dotvvmReturnedFile", StringComparison.Ordinal))
            {
                await RenderReturnedFile(request.HttpContext, request.Services.GetRequiredService<IReturnedFileStorage>());
                return true;
            }
            else return false;
        }

        private async Task RenderReturnedFile(IHttpContext context, IReturnedFileStorage returnedFileStorage)
        {
            ReturnedFileMetadata metadata;

            var id = Guid.Parse(context.Request.Query["id"]);
            using (var stream = returnedFileStorage.GetFile(id, out metadata))
            {
#if DotNetCore
                var contentDispositionValue = new ContentDispositionHeaderValue(metadata.AttachmentDispositionType);
                contentDispositionValue.SetHttpFileName(metadata.FileName);
                context.Response.Headers[HeaderNames.ContentDisposition] = contentDispositionValue.ToString();
#else
                var contentDispositionValue = new ContentDispositionHeaderValue(metadata.AttachmentDispositionType)
                {
                    FileName = metadata.FileName,
                    FileNameStar = metadata.FileName
                };
                context.Response.Headers["Content-Disposition"] = contentDispositionValue.ToString();
#endif
                context.Response.ContentType = metadata.MimeType;
                if (metadata.AdditionalHeaders != null)
                {
                    foreach (var header in metadata.AdditionalHeaders)
                    {
                        context.Response.Headers.Add(new KeyValuePair<string, string[]>(header.Key, header.Value));
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await stream.CopyToAsync(context.Response.Body);
            }
        }
    }
}
