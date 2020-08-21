using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.Setup.Configuration;
using NuGet.Frameworks;

namespace DotVVM.Cli
{
    public static class DotvvmProject
    {
        public const string DotvvmMetadataFile = ".dotvvm.json";
        public const string DotvvmPackage = "DotVVM";
        public const string DotvvmAssembly = "DotVVM.Framework";
        public const string DotvvmDirectory = ".dotvvm";
        public const string FallbackDotvvmVersion = "2.4.0.1";
        public const string PrintTargetFrameworkTarget = "PrintTargetFramework";

        public static DirectoryInfo CreateDotvvmDirectory(FileSystemInfo target)
        {
            if (target is FileInfo file)
            {
                target = file.Directory;
            }
            
            var dotvvmDir = new DirectoryInfo(Path.Combine(target.FullName, DotvvmDirectory));
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

            var directory = target switch
            {
                DirectoryInfo dir => dir,
                FileInfo file => file.Directory,
                _ => throw new NotImplementedException()
            };
            var metadata = new FileInfo(Path.Combine(directory.FullName, DotvvmMetadataFile));
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
                var json = await JsonSerializer.DeserializeAsync<ProjectMetadataJson>(stream);
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
                return await CreateProjectMetadata(target, logger, errorLevel);
            }

            logger.LogDebug($"Found DotVVM metadata at '{file}'.");
            var metadata = await LoadProjectMetadata(file, logger);
            if (metadata is null)
            {
                return await CreateProjectMetadata(target, logger, errorLevel);
            }
            return metadata;
        }

        public static async Task<ProjectMetadata?> CreateProjectMetadata(
            FileSystemInfo target,
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
            // TODO: Replace MSBuildLocator with a custom target (along with PrintTargetFrameworks)
            var msbuildInstance = MSBuildLocator.RegisterDefaults();
            if (msbuildInstance is null || string.IsNullOrEmpty(msbuildInstance.MSBuildPath))
            {
                logger.Log(errorLevel, $"Could not load MSBuild libraries.");
                return null;
            }

            logger.LogDebug($"Using MSBuild at '{msbuildInstance.MSBuildPath}' for project file inspection.");
            var metadata = CreateProjectMetadataFromMSBuild(projectFile, logger, errorLevel);
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

        public static ImmutableArray<NuGetFramework> GetTargetFrameworks(
            MSBuild msbuild,
            FileInfo project,
            ILogger? logger = null,
            LogLevel errorLevel = LogLevel.Critical)
        {
            logger ??= NullLogger.Instance;

            var dotvvmDir = CreateDotvvmDirectory(project);
            var printProject = new FileInfo(Path.Combine(
                dotvvmDir.FullName,
                $"{PrintTargetFrameworkTarget}.proj"));
            File.WriteAllText(printProject.FullName, GetPrintTargetFrameworkProject(project.FullName));

            var (success, stdout, stderr) = msbuild.TryInvokeWithOutput(
                project: printProject,
                targets: new [] {PrintTargetFrameworkTarget},
                logger: logger);
            if (!success)
            {
                logger.Log(errorLevel, $"The target framework of '{project}' could not be determined.");
                logger.LogDebug($"stdout:\n {stdout}");
                logger.LogDebug($"stderr:\n {stderr}");
                return ImmutableArray.Create<NuGetFramework>();
            }

            logger.LogDebug($"Target frameworks of '{project}' are '{stdout}'.");

            return stdout.Trim().Split(';').Select(NuGetFramework.Parse).ToImmutableArray();
        }

        private static string GetPrintTargetFrameworkProject(string projectPath)
        {
            return
$@"<Project>
  <Import Project=""{projectPath}"" />

  <Target Name=""{PrintTargetFrameworkTarget}"">
    <GetProjectTargetFrameworksTask
      ProjectPath=""$(MSBuildProjectFullPath""
      TargetFrameworks=""$(TargetFrameworks)""
      TargetFramework=""$(TargetFramework)""
      TargetFrameworkMoniker=""$(TargetFrameworkMoniker)""
      TargetPlatformIdentifier=""$(TargetPlatformIdentifier)""
      TargetPlatformVersion=""$(TargetPlatformVersion)""
      TargetPlatformMinVersion=""$(TargetPlatformMinVersion)"">
      <Output
        TaskParameter=""ProjectTargetFrameworks""
        PropertyName=""_ProjectTargetFrameworks""/>
    </GetProjectTargetFrameworksTask>
    <Message Text=""$(_ProjectTargetFrameworks)"" Importance=""High"" />
  </Target>
</Project>
";
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

        private static ProjectMetadata? CreateProjectMetadataFromMSBuild(
            FileInfo projectFile,
            ILogger logger,
            LogLevel errorLevel)
        {
            // Don't merge this function with CreateProjectMetadata. MSBuildLocator needs to be used before MSBuild.
            var project = Project.FromFile(projectFile.FullName, new ProjectOptions
            {
                LoadSettings = ProjectLoadSettings.IgnoreInvalidImports
                    | ProjectLoadSettings.IgnoreMissingImports
            });
            var msbuild = MSBuild.Create();
            if (msbuild is null)
            {
                logger.Log(errorLevel, "The MSBuild executable could not be found.");
                return null;
            }
            var targetFrameworks = GetTargetFrameworks(msbuild, projectFile, logger, errorLevel)
                .Select(t => t.GetShortFolderName())
                .ToImmutableArray();
            if (targetFrameworks.Length == 0)
            {
                return null;
            }

            return new ProjectMetadata(
                path: new FileInfo(Path.Combine(projectFile.DirectoryName, DotvvmMetadataFile)),
                projectName: project.GetPropertyValue("AssemblyName"),
                projectDirectory: projectFile.DirectoryName,
                rootNamespace: project.GetPropertyValue("RootNamespace"),
                packageVersion: GetDotvvmVersion(project),
                targetFrameworks: targetFrameworks,
                uiTestProjectPath: null,
                uiTestProjectRootNamespace: null,
                apiClients: ImmutableArray.Create<ApiClientDefinition>());
        }
        private static string GetDotvvmVersion(Project project)
        {
            var package = project.GetItems("PackageReference")
                .FirstOrDefault(p => p.EvaluatedInclude == DotvvmPackage);
            if (package is object)
            {
                return package.GetMetadataValue("Version");
            }

            var reference = project.GetItems("Reference")
                .Select(r => new AssemblyName(r.EvaluatedInclude))
                .FirstOrDefault(n => n.Name == DotvvmAssembly);
            if (reference is object && reference.Version is object)
            {
                return reference.Version.ToString();
            }

            return FallbackDotvvmVersion;
        }
    }
}
