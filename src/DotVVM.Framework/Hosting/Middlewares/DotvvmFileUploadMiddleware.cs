using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Storage;
using Microsoft.AspNet.WebUtilities;
using Newtonsoft.Json;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmFileUploadMiddleware : IMiddleware
    {
        private static readonly Regex baseMimeTypeRegex = new Regex(@"/.*$");
        private static readonly Regex wildcardMimeTypeRegex = new Regex(@"/\*$");

        private DotvvmConfiguration _configuration;

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
            _configuration = request.Configuration;
            var url = DotvvmMiddlewareBase.GetCleanRequestUrl(request.HttpContext);

            // file upload handler
            if (url == HostingConstants.FileUploadHandlerMatchUrl ||
                url.StartsWith(HostingConstants.FileUploadHandlerMatchUrl + "?", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessMultipartRequest(request.HttpContext);
                return true;
            }

            return false;
        }

        private async Task ProcessMultipartRequest(IHttpContext context)
        {
            // verify the request
            var isPost = context.Request.Method == "POST";
            if (isPost && !context.Request.ContentType.StartsWith("multipart/form-data", StringComparison.Ordinal))
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

        private bool ShouldReturnJsonResponse(IHttpContext context) =>
            context.Request.Headers[HostingConstants.DotvvmFileUploadAsyncHeaderName] == "true" ||
            context.Request.Query["returnJson"] == "true";

        private async Task RenderResponse(IHttpContext context, bool isPost, string errorMessage, List<UploadedFile> uploadedFiles)
        {
            var outputRenderer = _configuration.ServiceLocator.GetService<IOutputRenderer>();
            if (isPost && ShouldReturnJsonResponse(context))
            {
                // modern browser - return JSON
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (context.Request.Query["iframe"] == "true")
                    {
                        // IE will otherwise try to download the response as JSON file
                        await outputRenderer.RenderPlainTextResponse(context, JsonConvert.SerializeObject(uploadedFiles));
                    }
                    else
                    {
                        await outputRenderer.RenderPlainJsonResponse(context, uploadedFiles);
                    }
                }
                else
                {
                    await outputRenderer.RenderPlainTextResponse(context, errorMessage);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                // old browser - return HTML
                var template = new FileUploadPageTemplate {
                    FormPostUrl = context.Request.Url.ToString(),
                    AllowMultipleFiles = context.Request.Query["multiple"] == "true",
                    AllowedFileTypes = context.Request.Query["fileTypes"]
                };

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

        private async Task SaveFiles(IHttpContext context, Group boundary, List<UploadedFile> uploadedFiles)
        {
            // get the file store
            var fileStore = _configuration.ServiceLocator.GetService<IUploadedFileStorage>();

            // parse the stream
            var multiPartReader = new MultipartReader(boundary.Value, context.Request.Body);
            MultipartSection section;
            while ((section = await multiPartReader.ReadNextSectionAsync()) != null)
            {
                // process the section
                var result = await StoreFile(context, section, fileStore);
                if (result != null)
                {
                    uploadedFiles.Add(result);
                }
            }
        }

        /// <summary>
        /// Stores the file and returns an object that will be sent to the client.
        /// </summary>
        private async Task<UploadedFile> StoreFile(IHttpContext context, MultipartSection section, IUploadedFileStorage fileStore)
        {
            var fileId = await fileStore.StoreFile(section.Body);
            var fileNameGroup = Regex.Match(section.ContentDisposition, @"filename=""?(?<fileName>[^\""]*)", RegexOptions.IgnoreCase).Groups["fileName"];
            var fileName = fileNameGroup.Success ? fileNameGroup.Value : string.Empty;
            var mimeType = section.ContentType ?? string.Empty;
            
            return new UploadedFile {
                FileId = fileId,
                FileName = fileName,
                FileTypeAllowed = IsFileTypeAllowed(context, fileName, mimeType),
                MaxSizeExceeded = IsMaxSizeExceeded(context, section.Body)
            };
        }

        private bool IsFileTypeAllowed(IHttpContext context, string fileName, string mimeType)
        {
            var fileTypes = context.Request.Query["fileTypes"] 
                ?? context.Request.Query["accept"]; // for older BP versions

            if (string.IsNullOrEmpty(fileTypes))
            {
                return true;
            }

            return fileTypes.Split(',').Any(type =>
            {
                type = type.Trim();

                if (type.StartsWith(".", StringComparison.Ordinal))
                {
                    return string.Equals(type, Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase);
                }

                if (wildcardMimeTypeRegex.IsMatch(type))
                {
                    var baseMimeType = baseMimeTypeRegex.Replace(mimeType, string.Empty);
                    return baseMimeType == baseMimeTypeRegex.Replace(type, string.Empty);
                }

                if (mimeType.Length > 0)
                {
                    return type == mimeType;
                }

                return false;
            });
        }

        private bool IsMaxSizeExceeded(IHttpContext context, Stream body)
        {
            int maxSize;
            if (int.TryParse(context.Request.Query["maxSize"], out maxSize))
            {
                var maxSizeInBytes = maxSize * 1024 * 1024;
                return body.Length > maxSizeInBytes;
            }

            return false;
        }
    }
}