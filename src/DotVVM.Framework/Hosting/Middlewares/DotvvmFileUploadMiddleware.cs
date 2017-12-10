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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmFileUploadMiddleware : IMiddleware
    {
        private static readonly Regex baseMimeTypeRegex = new Regex(@"/.*$");
        private static readonly Regex wildcardMimeTypeRegex = new Regex(@"/\*$");
        private readonly IOutputRenderer outputRenderer;
        private readonly IUploadedFileStorage fileStorage;

        public DotvvmFileUploadMiddleware(IOutputRenderer outputRenderer, IUploadedFileStorage fileStorage)
        {
            this.outputRenderer = outputRenderer;
            this.fileStorage = fileStorage;
        }

        public static DotvvmFileUploadMiddleware TryCreate(IServiceProvider provider)
        {
            var renderer = provider.GetRequiredService<IOutputRenderer>();
            var fileStorage = provider.GetService<IUploadedFileStorage>();
            if (fileStorage != null)
                return new DotvvmFileUploadMiddleware(renderer, fileStorage);
            else
                return null;
        }

        public async Task<bool> Handle(IDotvvmRequestContext request)
        {
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
            // parse the stream
            var multiPartReader = new MultipartReader(boundary.Value, context.Request.Body);
            MultipartSection section;
            while ((section = await multiPartReader.ReadNextSectionAsync()) != null)
            {
                // process the section
                var result = await StoreFile(context, section, fileStorage);
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
            var fileSize = section.Body.Length;

            return new UploadedFile {
                FileId = fileId,
                FileName = fileName,
                FileSize = new FileSize{ Bytes = fileSize },
                IsFileTypeAllowed = IsFileTypeAllowed(context, fileName, mimeType),
                IsMaxSizeExceeded = IsMaxSizeExceeded(context, fileSize)
            };
        }

        private bool IsFileTypeAllowed(IHttpContext context, string fileName, string mimeType)
        {
            var allowedFileTypes = context.Request.Query["fileTypes"];

            if (string.IsNullOrEmpty(allowedFileTypes))
            {
                return true;
            }

            return allowedFileTypes.Split(',').Any(type =>
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

        private bool IsMaxSizeExceeded(IHttpContext context, long fileSize)
        {
            if (int.TryParse(context.Request.Query["maxSize"], out int maxFileSize))
            {
                var maxFileSizeInBytes = maxFileSize * 1024 * 1024;
                return fileSize > maxFileSizeInBytes;
            }

            return false;
        }
    }
}