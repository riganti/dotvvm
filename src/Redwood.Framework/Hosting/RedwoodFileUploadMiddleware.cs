using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Runtime;
using Redwood.Framework.Storage;

namespace Redwood.Framework.Hosting
{
    public class RedwoodFileUploadMiddleware : OwinMiddleware
    {
        private readonly RedwoodConfiguration configuration;


        public RedwoodFileUploadMiddleware(OwinMiddleware next, RedwoodConfiguration configuration) : base(next)
        {
            this.configuration = configuration;
        }

        public override async Task Invoke(IOwinContext context)
        {
            // try resolve the route
            var url = context.Request.Path.Value.TrimStart('/').TrimEnd('/');

            // file upload handler
            if (url == "redwoodFileUpload")
            {
                await ProcessMultipartRequest(context);
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        private async Task ProcessMultipartRequest(IOwinContext context)
        {
            // verify the request
            if (context.Request.Method != "POST")
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }
            if (!context.Request.ContentType.StartsWith("multipart/form-data"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // get the bounary
            var boundary = Regex.Match(context.Request.ContentType, @"boundary=""?(?<boundary>[^\n\;\"" ]*)").Groups["boundary"];
            if (!boundary.Success || string.IsNullOrWhiteSpace(boundary.Value))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            // get the file store
            var fileStore = configuration.ServiceLocator.GetService<IUploadedFileStorage>();

            // parse the stream
            var results = new List<UploadedFile>();
            var multiPartReader = new MultipartReader(boundary.Value, context.Request.Body);
            MultipartSection section;
            while ((section = await multiPartReader.ReadNextSectionAsync()) != null)
            {
                // process the section
                var result = await StoreFile(section, fileStore);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            // return the response
            var outputRenderer = configuration.ServiceLocator.GetService<IOutputRenderer>();
            await outputRenderer.RenderPlainJsonResponse(context, results);
        }

        /// <summary>
        /// Stores the file and returns an object that will be sent to the client.
        /// </summary>
        private async Task<UploadedFile> StoreFile(MultipartSection section, IUploadedFileStorage fileStore)
        {
            var fileId = await fileStore.StoreFile(section.Body);
            var fileName = Regex.Match(section.ContentDisposition, @"filename=""?(?<fileName>[^\""]*)", RegexOptions.IgnoreCase).Groups["fileName"];

            return new UploadedFile()
            {
                FileId = fileId,
                FileName = fileName.Success ? fileName.Value : string.Empty
            };
        }
    }
}
