using System;
using System.IO;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring DotVVM services.
    /// </summary>
    public interface IDotvvmBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> where DotVVM services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }

    public static class DotvvmBuilderExtensions
    {
        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder AddFileSystemTempFileStorages(this IDotvvmBuilder builder, string tempPath)
            => builder.AddFileSystemTempFileStorages(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder AddFileSystemTempFileStorages(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            builder.AddFileSystemUploadedFileStorage(tempPath, autoDeleteInterval);
            builder.AddFileSystemReturnedFileStorage(tempPath, autoDeleteInterval);
            return builder;
        }

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder AddFileSystemUploadedFileStorage(this IDotvvmBuilder builder, string tempPath)
            => builder.AddFileSystemUploadedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder AddFileSystemUploadedFileStorage(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            builder.Services.TryAddSingleton<IUploadedFileStorage>(s => new FileSystemUploadedFileStorage(Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath), autoDeleteInterval));
            return builder;
        }

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder AddFileSystemReturnedFileStorage(this IDotvvmBuilder builder, string tempPath)
            => builder.AddFileSystemReturnedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder AddFileSystemReturnedFileStorage(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            builder.Services.TryAddSingleton<IReturnedFileStorage>(s => new FileSystemReturnedFileStorage(Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath), autoDeleteInterval));
            return builder;
        }
    }
}