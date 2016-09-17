using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmReturnedFileMiddleware : IMiddleware
    {
        private readonly DotvvmConfiguration configuration;

        public Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next)
        {
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            return url.StartsWith("dotvvmReturnedFile", StringComparison.Ordinal) ? RenderReturnedFile(request.HttpContext) : next(request);
        }

        private async Task RenderReturnedFile(IHttpContext context)
        {
            var returnedFileStorage = configuration.ServiceLocator.GetService<IReturnedFileStorage>();
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