using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmReturnedFileMiddleware : OwinMiddleware
    {
        private readonly DotvvmConfiguration configuration;

        public DotvvmReturnedFileMiddleware(OwinMiddleware next, DotvvmConfiguration configuration) : base(next)
        {
            this.configuration = configuration;
        }

        public override Task Invoke(IOwinContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);
            
            return url.StartsWith("dotvvmReturnedFile", StringComparison.Ordinal) ? RenderReturnedFile(context) : Next.Invoke(context);
        }

        private async Task RenderReturnedFile(IOwinContext context)
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
                        context.Response.Headers.Add(header);
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await stream.CopyToAsync(context.Response.Body);
            }
        }
    }
}