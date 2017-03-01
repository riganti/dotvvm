using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.Middlewares
{ 
    //TODO: Code reveiw
    public class DotvvmReturnedFileMiddleware : IMiddleware
    {
        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            if (url.StartsWith("dotvvmReturnedFile", StringComparison.Ordinal))
            {
                await RenderReturnedFile(request.HttpContext, request.Services.GetService<IReturnedFileStorage>());
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
                context.Response.Headers["Content-Disposition"] = "attachment; filename=" + metadata.FileName;
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