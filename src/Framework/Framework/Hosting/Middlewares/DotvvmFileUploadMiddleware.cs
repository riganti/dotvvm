using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmFileUploadMiddleware : IMiddleware
    {
        private static readonly Regex baseMimeTypeRegex = new Regex(@"/.*$", RegexOptions.CultureInvariant);
        private static readonly Regex wildcardMimeTypeRegex = new Regex(@"/\*$", RegexOptions.CultureInvariant);
        private readonly IOutputRenderer outputRenderer;
        private readonly IUploadedFileStorage fileStorage;
        private readonly IViewModelSerializer viewModelSerializer;
        private readonly ICsrfProtector csrfProtector;
        private readonly IViewModelProtector viewModelProtector;

        public DotvvmFileUploadMiddleware(IOutputRenderer outputRenderer, IUploadedFileStorage fileStorage, IViewModelSerializer viewModelSerializer, ICsrfProtector csrfProtector, IViewModelProtector viewModelProtector)
        {
            this.outputRenderer = outputRenderer;
            this.fileStorage = fileStorage;
            this.viewModelSerializer = viewModelSerializer;
            this.csrfProtector = csrfProtector;
            this.viewModelProtector = viewModelProtector;
        }

        public static DotvvmFileUploadMiddleware? TryCreate(IServiceProvider provider)
        {
            var renderer = provider.GetRequiredService<IOutputRenderer>();
            var fileStorage = provider.GetService<IUploadedFileStorage>();
            var viewModelSerializer = provider.GetRequiredService<IViewModelSerializer>();
            var csrfProtector = provider.GetRequiredService<ICsrfProtector>();
            var viewModelProtector = provider.GetRequiredService<IViewModelProtector>();
            if (fileStorage != null)
                return new DotvvmFileUploadMiddleware(renderer, fileStorage, viewModelSerializer, csrfProtector, viewModelProtector);
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

            var uploadedFiles = new List<UploadedFile>();
            var errorMessage = "";
            var isPost = context.Request.Method == "POST";
            if (isPost)
            {
                var contentType = context.Request.ContentType;
                if (contentType is null || !contentType.StartsWith("multipart/form-data", StringComparison.Ordinal))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                // verify the request
                var csrfToken = context.Request.Headers["X-DotVVM-CsrfToken"];
                if (csrfToken is null or "")
                    await request.RejectRequest("Missing CSRF token");

                try
                {
                    csrfProtector.VerifyToken(request, csrfToken);
                }
                catch (CorruptedCsrfTokenException)
                {
                    await request.RejectRequest("Corrupted CSRF token");
                }
                catch (SecurityException)
                {
                    await request.RejectRequest("Corrupted CSRF token");
                }

                var authorizeFileUpload = request.Configuration.Security.AuthorizeFileUpload;
                if (authorizeFileUpload is {} && !await authorizeFileUpload(request))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                var site = context.Request.Headers["Sec-Fetch-Site"];
                if (!string.IsNullOrEmpty(site) && site != "same-origin")
                    await request.RejectRequest("DotVVM file upload: Sec-Fetch-Site must be same-origin");

                try
                {
                    var uploadTokenHeader = context.Request.Headers["X-DotVVM-UploadToken"];
                    if (string.IsNullOrEmpty(uploadTokenHeader))
                        await request.RejectRequest("Missing X-DotVVM-UploadToken");

                    var tokenBytes = Convert.FromBase64String(uploadTokenHeader!);
                    var unprotected = viewModelProtector.Unprotect(tokenBytes, "FileUpload", ProtectionHelpers.GetUserIdentity(request));
                    var uploadToken = JsonSerializer.Deserialize<FileUploadToken>(unprotected.AsSpan());

                    if (uploadToken is null)
                        await request.RejectRequest("Invalid X-DotVVM-UploadToken");

                    // get the boundary
                    var boundary = Regex.Match(contentType, @"boundary=""?(?<boundary>[^\n\;\"" ]*)", RegexOptions.CultureInvariant).Groups["boundary"];
                    if (!boundary.Success || string.IsNullOrWhiteSpace(boundary.Value))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    // parse request and save files
                    await SaveFiles(request, boundary, uploadedFiles, uploadToken);
                }
                catch (Exception ex) when (ex is not DotvvmInterruptRequestExecutionException)
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
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await outputRenderer.RenderPlainTextResponse(context, errorMessage);
                }
            }

        }

        private async Task SaveFiles(IDotvvmRequestContext context, Group boundary, List<UploadedFile> uploadedFiles, FileUploadToken token)
        {
            if (context.HttpContext.Request.Headers["X-DotVVM-UploadChunk"] == "true")
            {
                await SaveChunk(context, boundary.Value, uploadedFiles, token);
                return;
            }
            if (token.FileId != null || token.NextPosition != 0)
                await context.RejectRequest("A continuation token requires a chunk upload.");

            // parse the stream
            var multiPartReader = new MultipartReader(boundary.Value, context.HttpContext.Request.Body);
            while (await multiPartReader.ReadNextSectionAsync() is MultipartSection section)
            {
                // process the section
                var result = await StoreFile(context, section, fileStorage, token);
                if (result != null)
                {
                    uploadedFiles.Add(result);
                }
            }
        }

        private async Task SaveChunk(IDotvvmRequestContext context, string boundary, List<UploadedFile> uploadedFiles, FileUploadToken token)
        {
            if (fileStorage is not IChunkedUploadedFileStorage)
                await context.RejectRequest("The file storage does not support chunk uploads.");
            var storage = (IChunkedUploadedFileStorage)fileStorage;
            var chunkSize = context.Configuration.Runtime.FileUploadChunkSizeBytes;
            if (chunkSize <= 0)
                await context.RejectRequest("Invalid chunk upload headers.");
            if (token.MaxFileSize == null || token.MaxFileSize < 0)
                await context.RejectRequest("MaxFileSize must be specified and nonnegative for chunked uploads.");
            if (token.NextPosition < 0 || (token.FileId == null ? token.NextPosition != 0 : token.NextPosition <= 0))
                await context.RejectRequest("Invalid upload continuation.");

            var totalHeader = context.HttpContext.Request.Headers["X-DotVVM-UploadFileSize"];
            if (token.FileId == null)
            {
                if (string.IsNullOrEmpty(totalHeader) || totalHeader.Any(c => c < '0' || c > '9') ||
                    !long.TryParse(totalHeader, NumberStyles.None, CultureInfo.InvariantCulture, out var totalSize))
                    await context.RejectRequest("Invalid X-DotVVM-UploadFileSize.");
                else
                    token.TotalFileSize = totalSize;
            }
            else if (!string.IsNullOrEmpty(totalHeader) || token.TotalFileSize == null ||
                token.TotalFileSize < 0 || token.NextPosition >= token.TotalFileSize)
                await context.RejectRequest("Invalid upload continuation.");

            var total = token.TotalFileSize!.Value;
            if (token.MaxFileSize is {} declaredMax && declaredMax >= 0 && total > declaredMax)
                await context.RejectRequest("Max file size exceeded", statusCode: 413);

            var reader = new MultipartReader(boundary, context.HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            if (section == null)
                await context.RejectRequest("A chunk upload must contain exactly one file.");
            var fileNameGroup = Regex.Match(section!.ContentDisposition ?? "", @"filename=""?(?<fileName>[^\""]*)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Groups["fileName"];
            if (!fileNameGroup.Success)
                await context.RejectRequest("A chunk upload must contain a file.");
            var fileName = fileNameGroup.Value;
            var mimeType = section.ContentType ?? "";
            if (token.FileId != null && (token.FileName != fileName || token.MimeType != mimeType))
                await context.RejectRequest("Upload continuation metadata does not match.");

            var remaining = token.MaxFileSize is {} maxSize && maxSize >= 0
                ? maxSize - token.NextPosition
                : long.MaxValue;
            if (remaining < 0)
                await context.RejectRequest("Max file size exceeded", statusCode: 413);
            using var buffer = new MemoryStream();
            try
            {
                await LimitLengthStream.LimitLength(section.Body, Math.Min(chunkSize, Math.Min(total - token.NextPosition, remaining)), "Upload chunk size limit").CopyToAsync(buffer);
            }
            catch (LimitLengthStream.LimitExceededException)
            {
                await context.RejectRequest("Upload chunk or max file size exceeded", statusCode: 413);
                throw;
            }
            // Validate the entire multipart envelope before changing the stored file.
            if (await reader.ReadNextSectionAsync() != null)
                await context.RejectRequest("A chunk upload must contain exactly one file.");
            var position = checked(token.NextPosition + buffer.Length);
            var isFinal = position == total;
            if (buffer.Length == 0 && (!isFinal || token.FileId != null))
                await context.RejectRequest("An upload continuation or nonfinal chunk must not be empty.");

            buffer.Position = 0;
            var fileId = token.FileId;
            if (fileId is {} existingId)
                await storage.AppendFileAsync(existingId, token.NextPosition, buffer);
            else
                fileId = await storage.StoreFileAsync(buffer);
            if (isFinal)
            {
                DotvvmMetrics.UploadedFileSize.Record(position);
                uploadedFiles.Add(new UploadedFile {
                    FileId = fileId.Value,
                    FileName = fileName,
                    FileSize = new FileSize { Bytes = position },
                    IsFileTypeAllowed = IsFileTypeAllowed(fileName, mimeType, token),
                    IsMaxSizeExceeded = false
                });
            }
            else
            {
                token.FileId = fileId;
                token.NextPosition = position;
                token.FileName = fileName;
                token.MimeType = mimeType;
                var json = JsonSerializer.SerializeToUtf8Bytes(token, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                context.HttpContext.Response.Headers["X-DotVVM-UploadToken"] = Convert.ToBase64String(
                    viewModelProtector.Protect(json, "FileUpload", ProtectionHelpers.GetUserIdentity(context)));
            }
        }

        /// <summary>
        /// Stores the file and returns an object that will be sent to the client.
        /// </summary>
        private async Task<UploadedFile> StoreFile(IDotvvmRequestContext context, MultipartSection section, IUploadedFileStorage fileStore, FileUploadToken token)
        {
            var fileNameGroup = Regex.Match(section.ContentDisposition, @"filename=""?(?<fileName>[^\""]*)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Groups["fileName"];
            var fileName = fileNameGroup.Success ? fileNameGroup.Value : string.Empty;
            var mimeType = section.ContentType ?? string.Empty;

            Guid fileId;
            try
            {
                var fileStream = token.MaxFileSize is {} maxSize
                    ? LimitLengthStream.LimitLength(section.Body, maxSize, "File size limit")
                    : section.Body;
                fileId = await fileStore.StoreFileAsync(fileStream);
            }
            catch (LimitLengthStream.LimitExceededException)
            {
                await context.RejectRequest("Max file size exceeded", statusCode: 413);
                throw;
            }
            var fileSize = section.Body.Length;
            DotvvmMetrics.UploadedFileSize.Record(fileSize);

            return new UploadedFile {
                FileId = fileId,
                FileName = fileName,
                FileSize = new FileSize{ Bytes = fileSize },
                IsFileTypeAllowed = IsFileTypeAllowed(fileName, mimeType, token),
                IsMaxSizeExceeded = false,
            };
        }

        private bool IsFileTypeAllowed(string fileName, string mimeType, FileUploadToken token)
        {
            var allowedFileTypes = token.AllowedFileTypes;

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
    }
}
