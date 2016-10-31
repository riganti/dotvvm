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
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder ConfigureTempStorages(this IDotvvmBuilder builder, string tempPath)
            => builder.ConfigureTempStorages(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder ConfigureTempStorages(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            return builder
                .ConfigureUploadedFileStorage(Path.Combine(tempPath, "uploadedFiles"), autoDeleteInterval)
                .ConfigureReturnedFileStorage(Path.Combine(tempPath, "returnedFiles"), autoDeleteInterval);
        }

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder ConfigureUploadedFileStorage(this IDotvvmBuilder builder, string tempPath)
            => builder.ConfigureUploadedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder ConfigureUploadedFileStorage(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            builder.Services.TryAddSingleton<IUploadedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemUploadedFileStorage(fullPath, autoDeleteInterval);
            });
            return builder;
        }

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmBuilder ConfigureReturnedFileStorage(this IDotvvmBuilder builder, string tempPath)
            => builder.ConfigureReturnedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmBuilder ConfigureReturnedFileStorage(this IDotvvmBuilder builder, string tempPath, TimeSpan autoDeleteInterval)
        {
            builder.Services.TryAddSingleton<IReturnedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemReturnedFileStorage(fullPath, autoDeleteInterval);
            });
            return builder;
        }

        /// <summary>
        /// Runs a custom configuration task on the <see cref="IDotvvmBuilder" /> object.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        /// <param name="configureAction">A method which configures the builder.</param>
        public static IDotvvmBuilder Configure(this IDotvvmBuilder builder, Action<IDotvvmBuilder> configureAction)
        {
            configureAction(builder);
            return builder;
        }

        /// <summary>
        /// Indicates that the DotVVM configuration ends and allows to continue with the configuration of the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IDotvvmBuilder" /> instance.</param>
        public static IServiceCollection Done(this IDotvvmBuilder builder)
        {
            return builder.Services;
        }
    }
}