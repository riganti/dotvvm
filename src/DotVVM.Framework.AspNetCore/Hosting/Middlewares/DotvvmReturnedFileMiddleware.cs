using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmReturnedFileMiddleware
    {
        private readonly DotvvmConfiguration configuration;
		private readonly RequestDelegate next;

		public DotvvmReturnedFileMiddleware(RequestDelegate next, DotvvmConfiguration configuration)
        {
			this.next = next;
            this.configuration = configuration;
        }

        public Task Invoke(HttpContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);
            
            return url.StartsWith("dotvvmReturnedFile", StringComparison.Ordinal) ? RenderReturnedFile(context) : next(context);
        }

        private async Task RenderReturnedFile(HttpContext context)
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
                        context.Response.Headers.Add(new KeyValuePair<string, StringValues>(header.Key, new StringValues(header.Value)));
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await stream.CopyToAsync(context.Response.Body);
            }
        }
    }
}