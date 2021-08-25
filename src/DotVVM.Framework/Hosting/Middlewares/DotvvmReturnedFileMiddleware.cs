using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Core.Storage;
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
            var id = Guid.Parse(context.Request.Query["id"]);

            var returnedFile = await returnedFileStorage.GetFileAsync(id);
            using (var stream = returnedFile.Stream)
            {
#if DotNetCore
                var contentDispositionValue = new ContentDispositionHeaderValue(returnedFile.Metadata.AttachmentDispositionType);
                contentDispositionValue.SetHttpFileName(returnedFile.Metadata.FileName);
                context.Response.Headers[HeaderNames.ContentDisposition] = contentDispositionValue.ToString();
#else
                var contentDispositionValue = new ContentDispositionHeaderValue(returnedFile.Metadata.AttachmentDispositionType)
                {
                    FileName = returnedFile.Metadata.FileName,
                    FileNameStar = returnedFile.Metadata.FileName
                };
                context.Response.Headers["Content-Disposition"] = contentDispositionValue.ToString();
#endif
                context.Response.ContentType = returnedFile.Metadata.MimeType;
                if (returnedFile.Metadata.AdditionalHeaders != null)
                {
                    foreach (var header in returnedFile.Metadata.AdditionalHeaders)
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
