using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ViewModel.Serialization;
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
        private readonly IViewModelSerializer viewModelSerializer;

        public DotvvmFileUploadMiddleware(IOutputRenderer outputRenderer, IUploadedFileStorage fileStorage, IViewModelSerializer viewModelSerializer)
        {
            this.outputRenderer = outputRenderer;
            this.fileStorage = fileStorage;
            this.viewModelSerializer = viewModelSerializer;
        }

        public static DotvvmFileUploadMiddleware? TryCreate(IServiceProvider provider)
        {
            var renderer = provider.GetRequiredService<IOutputRenderer>();
            var fileStorage = provider.GetService<IUploadedFileStorage>();
            var viewModelSerializer = provider.GetRequiredService<IViewModelSerializer>();
            if (fileStorage != null)
                return new DotvvmFileUploadMiddleware(renderer, fileStorage, viewModelSerializer);
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
                await ProcessMultipartRequest(request);
                return true;
            }

            return false;
        }

        private async Task ProcessMultipartRequest(IDotvvmRequestContext request)
        {
            var context = request.HttpContext;

            // verify the request
            var isPost = context.Request.Method == "POST";
            if (isPost && !context.Request.ContentType!.StartsWith("multipart/form-data", StringComparison.Ordinal))
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
            await RenderResponse(request, isPost, errorMessage, uploadedFiles);
        }

        private async Task RenderResponse(IDotvvmRequestContext request, bool isPost, string errorMessage, List<UploadedFile> uploadedFiles)
        {
            var context = request.HttpContext;

            if (isPost)
            {
                // modern browser - return JSON
                if (string.IsNullOrEmpty(errorMessage))
                {
                    var json = viewModelSerializer.BuildStaticCommandResponse(request, uploadedFiles);
                    await outputRenderer.RenderPlainJsonResponse(context, json);
                }
                else
                {
                    await outputRenderer.RenderPlainTextResponse(context, errorMessage);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            
        }

        private async Task SaveFiles(IHttpContext context, Group boundary, List<UploadedFile> uploadedFiles)
        {
            // parse the stream
            var multiPartReader = new MultipartReader(boundary.Value, context.Request.Body);
            while (await multiPartReader.ReadNextSectionAsync() is MultipartSection section)
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
            var fileId = await fileStore.StoreFileAsync(section.Body);
            var fileNameGroup = Regex.Match(section.ContentDisposition, @"filename=""?(?<fileName>[^\""]*)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Groups["fileName"];
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
