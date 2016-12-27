using System;
using System.IO;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DotvvmBuilderExtensions
    {
        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmOptions AddDefaultTempStorages(this IDotvvmOptions options, string tempPath)
            => options.AddDefaultTempStorages(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmOptions AddDefaultTempStorages(this IDotvvmOptions options, string tempPath, TimeSpan autoDeleteInterval)
        {
            return options
                .AddUploadedFileStorage(Path.Combine(tempPath, "uploadedFiles"), autoDeleteInterval)
                .AddReturnedFileStorage(Path.Combine(tempPath, "returnedFiles"), autoDeleteInterval);
        }

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmOptions AddUploadedFileStorage(this IDotvvmOptions options, string tempPath)
            => options.AddUploadedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmOptions AddUploadedFileStorage(this IDotvvmOptions options, string tempPath, TimeSpan autoDeleteInterval)
        {
            options.Services.TryAddSingleton<IUploadedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemUploadedFileStorage(fullPath, autoDeleteInterval);
            });
            return options;
        }

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmOptions AddReturnedFileStorage(this IDotvvmOptions options, string tempPath)
            => options.AddReturnedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="options">The <see cref="IDotvvmOptions" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmOptions AddReturnedFileStorage(this IDotvvmOptions options, string tempPath, TimeSpan autoDeleteInterval)
        {
            options.Services.TryAddSingleton<IReturnedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemReturnedFileStorage(fullPath, autoDeleteInterval);
            });
            return options;
        }
        
    }
}