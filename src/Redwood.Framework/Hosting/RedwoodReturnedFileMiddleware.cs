using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Storage;

namespace Redwood.Framework.Hosting
{
    public class RedwoodReturnedFileMiddleware : OwinMiddleware
    {
        private readonly RedwoodConfiguration _configuration;

        public RedwoodReturnedFileMiddleware(OwinMiddleware next, RedwoodConfiguration configuration) : base(next)
        {
            _configuration = configuration;
        }

        public override Task Invoke(IOwinContext context)
        {
            var url = RedwoodMiddleware.GetCleanRequestUrl(context);
            
            return url.StartsWith("redwoodReturnedFile") ? RenderReturnedFile(context) : Next.Invoke(context);
        }

        private async Task RenderReturnedFile(IOwinContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.OK;
            var id = context.Request.Query["id"];

            var returnedFileStorage = _configuration.ServiceLocator.GetService<IReturnedFileStorage>();
            string fileName, mimeType;
            IHeaderDictionary additionalHeaders;

            using (var stream = returnedFileStorage.GetFile(id, out fileName, out mimeType, out additionalHeaders))
            {
                context.Response.Headers["Content-Disposition"] = "attachment; filename=" + fileName;
                context.Response.ContentType = mimeType;
                foreach (var header in additionalHeaders)
                {
                    context.Response.Headers.Add(header);
                }

                await stream.CopyToAsync(context.Response.Body);
            }
        }
    }
}