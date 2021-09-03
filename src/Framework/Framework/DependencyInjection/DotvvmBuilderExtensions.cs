using System;
using System.IO;
using DotVVM.Core.Storage;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Tracing;
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
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmServiceCollection AddDefaultTempStorages(this IDotvvmServiceCollection services, string tempPath)
            => services.AddDefaultTempStorages(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system temporary file storages to the application. See <see cref="IUploadedFileStorage" />
        /// and <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmServiceCollection AddDefaultTempStorages(this IDotvvmServiceCollection services, string tempPath, TimeSpan autoDeleteInterval)
        {
            return services
                .AddUploadedFileStorage(Path.Combine(tempPath, "uploadedFiles"), autoDeleteInterval)
                .AddReturnedFileStorage(Path.Combine(tempPath, "returnedFiles"), autoDeleteInterval);
        }

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmServiceCollection AddUploadedFileStorage(this IDotvvmServiceCollection services, string tempPath)
            => services.AddUploadedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system uploaded file storage to the application. See <see cref="IUploadedFileStorage" /> for more details.
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmServiceCollection AddUploadedFileStorage(this IDotvvmServiceCollection services, string tempPath, TimeSpan autoDeleteInterval)
        {
            services.Services.TryAddSingleton<IUploadedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetRequiredService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemUploadedFileStorage(fullPath, autoDeleteInterval);
            });
            return services;
        }

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        public static IDotvvmServiceCollection AddReturnedFileStorage(this IDotvvmServiceCollection services, string tempPath)
            => services.AddReturnedFileStorage(tempPath, TimeSpan.FromMinutes(30));

        /// <summary>
        /// Adds file system returned file storage to the application. See <see cref="IReturnedFileStorage" /> for more details.
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        /// <param name="tempPath">The absolute or relative path to directory where to store temporary files.</param>
        /// <param name="autoDeleteInterval">The interval to delete the temporary files after.</param>
        public static IDotvvmServiceCollection AddReturnedFileStorage(this IDotvvmServiceCollection services, string tempPath, TimeSpan autoDeleteInterval)
        {
            services.Services.TryAddSingleton<IReturnedFileStorage>(s =>
            {
                var fullPath = Path.Combine(s.GetRequiredService<DotvvmConfiguration>().ApplicationPhysicalPath, tempPath);
                return new FileSystemReturnedFileStorage(fullPath, autoDeleteInterval);
            });
            return services;
        }

        /// <summary>
        /// Adds diagnostic services required for DotVVM Diagnostic Window to work.
        /// DotVVM Diagnostic Window helps with debugging postbacks and highlighting requests with potential issues during development time. 
        /// </summary>
        /// <param name="services">The <see cref="IDotvvmServiceCollection" /> instance.</param>
        public static IDotvvmServiceCollection AddDiagnosticServices(this IDotvvmServiceCollection services)
        {
            services.Services.TryAddSingleton<DiagnosticsServerConfiguration>();
            services.Services.TryAddSingleton<IDiagnosticsInformationSender, DiagnosticsInformationSender>();

            services.Services.TryAddSingleton<IOutputRenderer, DiagnosticsRenderer>();

            services.Services.AddScoped<DiagnosticsRequestTracer>();
            services.Services.AddScoped<IRequestTracer>(s => {
                var config = s.GetRequiredService<DotvvmConfiguration>();
                return (config.Debug ? (IRequestTracer)s.GetService<DiagnosticsRequestTracer>() : null) ?? new NullRequestTracer();
            });

            return services;
        }
    }
}
