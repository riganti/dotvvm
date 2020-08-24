using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public static class DotvvmProject
    {
        public const string MetadataFilename = "metadata.json";
        public const string CliDirectoryName = ".dotvvm";
        public const string FallbackVersion = "2.4.0.1";
        public const string WriteDotvvmMetadataTarget = "_WriteProjectMetadata";

        public static DirectoryInfo CreateDotvvmDirectory(FileSystemInfo target)
        {
            if (target is FileInfo file)
            {
                target = file.Directory;
            }
            
            var dotvvmDir = new DirectoryInfo(Path.Combine(target.FullName, CliDirectoryName));
            if (!dotvvmDir.Exists)
            {
                dotvvmDir.Create();
            }

            return dotvvmDir;
        }

        public static FileInfo? FindProjectMetadata(FileSystemInfo target)
        {
            if (!target.Exists)
            {
                return null;
            }

            var metadata = GetCliFile(target, MetadataFilename);
            return metadata.Exists ? metadata : null;
        }

        public static async Task<ProjectMetadata?> LoadProjectMetadata(
            FileInfo file,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Debug)
        {
            logger ??= NullLogger.Instance;

            try
            {
                using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                var json = await JsonSerializer.DeserializeAsync<ProjectMetadataJsonOld>(stream);
                var error = ProjectMetadata.IsJsonValid(json);
                if (error is object)
                {
                    logger.Log(errorLevel, error, "DotVVM metadata are not valid.");
                    return null;
                }
                return ProjectMetadata.FromJson(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static async Task<ProjectMetadata?> GetProjectMetadata(
            FileSystemInfo target,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var file = FindProjectMetadata(target);
            if (file is null)
            {
                return await CreateProjectMetadata(target, true, logger, errorLevel);
            }

            logger.LogDebug($"Found DotVVM metadata at '{file}'.");
            var metadata = await LoadProjectMetadata(file, logger);
            if (metadata is null)
            {
                return await CreateProjectMetadata(target, true, logger, errorLevel);
            }
            return metadata;
        }

        public static async Task<ProjectMetadata?> CreateProjectMetadata(
            FileSystemInfo target,
            bool showMSBuildOutput = false,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var projectFile = ProjectFile.FindProjectFile(target);
            if (projectFile is null)
            {
                logger.Log(errorLevel, $"No project could be found at '{target}'.");
                return null;
            }

            logger.LogDebug($"Found a project file at '{projectFile}'.");
            var msbuild = MSBuild.Create();
            if (msbuild is null)
            {
                logger.Log(errorLevel, "Could not found an MSBuild executable.");
                return null;
            }
            logger.LogDebug($"Found the '{msbuild}' MSBuild executable.");

            var metadataFile = WriteDotvvmMetadata(msbuild, projectFile, showMSBuildOutput, logger, errorLevel);
            if (metadataFile is null)
            {
                return null;
            }

            var metadata = await LoadProjectMetadata(metadataFile, logger, errorLevel);
            if (metadata is null)
            {
                return null;
            }

            await SaveProjectMetadata(metadata);
            logger.LogDebug($"Saved DotVVM metadata to '{metadata.Path}'.");
            return metadata;
        }

        public static async Task SaveProjectMetadata(ProjectMetadata metadata)
        {
            using var stream = metadata.Path.Open(FileMode.Create, FileAccess.Write);
            var json = metadata.ToJson();
            json.Version = 2; // TODO: Why?
            await JsonSerializer.SerializeAsync(stream, json, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static DotvvmConfiguration GetConfiguration(string projectName, string projectDirectory)
        {
            return GetConfiguration(
                webSiteAssembly: Assembly.Load(projectName),
                webSitePath: projectDirectory,
                configureServices: c => c.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());
        }

        public static DotvvmConfiguration GetConfiguration(
            Assembly webSiteAssembly,
            string webSitePath,
            Action<IServiceCollection> configureServices)
        {
            var dotvvmStartup = GetDotvvmStartup(webSiteAssembly);
            var configuratorType = GetDotvvmServiceConfiguratorType(webSiteAssembly);
            var configureServicesMethod = configuratorType is object
                ? GetConfigureServicesMethod(configuratorType) 
                : null;

            var config = DotvvmConfiguration.CreateDefault(services => {
                if (configureServicesMethod is object)
                {
                    InvokeConfigureServices(configureServicesMethod, services);
                }
                configureServices?.Invoke(services);
            });

            config.ApplicationPhysicalPath = webSitePath;
            config.CompiledViewsAssemblies = null!;

            //configure dotvvm startup
            dotvvmStartup?.Configure(config, webSitePath);

            return config;
        }

        public static IDotvvmStartup GetDotvvmStartup(Assembly assembly)
        {
            //find all implementations of IDotvvmStartup
            var dotvvmStartupType = GetDotvvmStartupType(assembly);
            if(dotvvmStartupType is null)
            {
                throw new ArgumentException("Could not found an implementation of IDotvvmStartup "
                    + $"in '{assembly.FullName}.");
            }

            return dotvvmStartupType.Apply(Activator.CreateInstance)!.CastTo<IDotvvmStartup>();
        }

        public static FileInfo GetCliFile(FileSystemInfo target, string relativePath)
        {
            var dir = CreateDotvvmDirectory(target);
            return new FileInfo(Path.Combine(dir.FullName, relativePath));
        }

        private static FileInfo? WriteDotvvmMetadata(
            MSBuild msbuild,
            FileInfo project,
            bool showMSBuildOutput,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var writeMetadataProjectFile = GetCliFile(project, $"{WriteDotvvmMetadataTarget}.proj");
            File.WriteAllText(writeMetadataProjectFile.FullName, GetWriteDotvvmMetadataProject());

            var success = msbuild.TryInvoke(
                project: project,
                properties: new []
                {
                    new KeyValuePair<string, string>(
                        "CustomBeforeMicrosoftCommonTargets",
                        writeMetadataProjectFile.FullName),
                    new KeyValuePair<string, string>("IsCrossTargetingBuild", "false")
                },
                targets: new [] {WriteDotvvmMetadataTarget},
                showOutput: showMSBuildOutput,
                logger: logger);
            if (!success)
            {
                logger.Log(errorLevel, $"The DotVVM metadata of '{project}' could not be determined.");
                return null;
            }

            return FindProjectMetadata(project);
        }

        private static string GetWriteDotvvmMetadataProject()
        {
            using var stream = typeof(DotvvmProject).Assembly
                .GetManifestResourceStream("DotVVM.CommandLine.Common.WriteProjectMetadata.targets");
            if (stream is null)
            {
                throw new InvalidOperationException("Could not read the embedded WriteProjectMetadata.targets file.");
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static Type? GetDotvvmStartupType(Assembly assembly)
        {
            var dotvvmStartups = assembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (dotvvmStartups.Length > 1)
            {
                var startupNames = string.Join(", ", dotvvmStartups.Select(s => $"'{s.Name}'"));
                throw new ArgumentException("Found more than one IDotvvmStartup implementation in "
                    + $"'{assembly.FullName}': {startupNames}.");
            }
            return dotvvmStartups.SingleOrDefault();
        }

        private static Type? GetDotvvmServiceConfiguratorType(Assembly assembly)
        {
            var interfaceType = typeof(IDotvvmServiceConfigurator);
            var resultTypes = assembly.GetLoadableTypes()
                .Where(s => s.GetTypeInfo().ImplementedInterfaces
                    .Any(i => i.Name == interfaceType.Name))
                    .Where(s => s != null)
                .ToArray();
            if (resultTypes.Length > 1)
            {
                throw new ArgumentException("Found more than one implementation of IDotvvmServiceConfiguration in "
                    + $"'{assembly.FullName}'.");
            }

            return resultTypes.SingleOrDefault();
        }

        private static MethodInfo GetConfigureServicesMethod(Type type)
        {
            var method = type.GetMethod("ConfigureServices", new[] {typeof(IDotvvmServiceCollection)});
            if (method == null)
            {
                throw new ArgumentException($"Type '{type}' is missing the "
                    + "'void ConfigureServices IDotvvmServiceCollection services)'.");
            }
            return method;
        }

        private static void InvokeConfigureServices(MethodInfo method, IServiceCollection collection)
        {
            if (method.IsStatic)
            {
                method.Invoke(null, new object[] {new DotvvmServiceCollection(collection)});
            }
            else
            {
                var instance = Activator.CreateInstance(method.DeclaringType!);
                method.Invoke(instance, new object[] { new DotvvmServiceCollection(collection) });
            }
        }
    }
}
