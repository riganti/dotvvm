using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Owin;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Storage;
using DotVVM.Framework.Routing.Hosting;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmFileUploadMiddleware : OwinMiddleware
    {
        private readonly DotvvmConfiguration configuration;


        public DotvvmFileUploadMiddleware(OwinMiddleware next, DotvvmConfiguration configuration) : base(next)
        {
            this.configuration = configuration;
        }

        public override Task Invoke(IOwinContext context)
        {
            var url = DotvvmMiddleware.GetCleanRequestUrl(context);
            
            // file upload handler
            if (url == Constants.FileUploadHandlerMatchUrl)
            {
                return ProcessMultipartRequest(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }

        private async Task ProcessMultipartRequest(IOwinContext context)
        {
            // verify the request
            var isPost = context.Request.Method == "POST";
            if (isPost && !context.Request.ContentType.StartsWith("multipart/form-data"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var uploadedFiles = new List<UploadedFile>();
            var errorMessage = "";
            if (isPost)
            {
                try
                {
                    // get the boundary
                    var boundary = Regex.Match(context.Request.ContentType, @"boundary=""?(?<boundary>[^\n\;\"" ]*)").Groups["boundary"];
                    if (!boundary.Success || string.IsNullOrWhiteSpace(boundary.Value))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    // parse request and save files
                    await SaveFiles(context, boundary, uploadedFiles);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            // return the response
            await RenderResponse(context, isPost, errorMessage, uploadedFiles);
        }

        private async Task RenderResponse(IOwinContext context, bool isPost, string errorMessage, List<UploadedFile> uploadedFiles)
        {
            var outputRenderer = configuration.ServiceLocator.GetService<IOutputRenderer>();
            if (isPost && context.Request.Headers.Get(Constants.DotvvmFileUploadAsyncHeaderName) == "true")
            {
                // modern browser - return JSON
                if (string.IsNullOrEmpty(errorMessage))
                {
                    await outputRenderer.RenderPlainJsonResponse(context, uploadedFiles);
                }
                else
                {
                    await outputRenderer.RenderPlainTextResponse(context, errorMessage);
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                // old browser - return HTML
                var template = new FileUploadPageTemplate();
                template.FormPostUrl = DotvvmRequestContext.TranslateVirtualPath("~/" + Constants.FileUploadHandlerMatchUrl, context);
                template.AllowMultipleFiles = context.Request.Query["multiple"] == "true";

                if (isPost)
                {
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        template.StartupScript = string.Format("reportProgress(false, 100, {0})",
                            JsonConvert.SerializeObject(uploadedFiles));
                    }
                    else
                    {
                        template.StartupScript = string.Format("reportProgress(false, 100, {0})",
                            JsonConvert.SerializeObject(errorMessage));
                    }
                }
                await outputRenderer.RenderHtmlResponse(context, template.TransformText());
            }
        }

        private async Task SaveFiles(IOwinContext context, Group boundary, List<UploadedFile> uploadedFiles)
        {
            // get the file store
            var fileStore = configuration.ServiceLocator.GetService<IUploadedFileStorage>();

            // parse the stream
            var multiPartReader = new MultipartReader(boundary.Value, context.Request.Body);
            MultipartSection section;
            while ((section = await multiPartReader.ReadNextSectionAsync()) != null)
            {
                // process the section
                var result = await StoreFile(section, fileStore);
                if (result != null)
                {
                    uploadedFiles.Add(result);
                }
            }
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
