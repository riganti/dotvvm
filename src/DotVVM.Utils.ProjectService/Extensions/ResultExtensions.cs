using System.Collections.Generic;
using System.IO;
using DotVVM.Utils.ProjectService.Operations;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Extensions
{
    public static class ResultExtensions
    {
        public static ResolvedProjectStatistics ToStatisticsResult(this IResolvedProjectMetadata resolvedMetadata)
        {
            return new ResolvedProjectStatistics()
            {
                CsprojFullName = resolvedMetadata.CsprojFullName,
                CsprojVersion = resolvedMetadata.CsprojVersion,
                DotvvmProjectDependencies = resolvedMetadata.DotvvmProjectDependencies,
                OperationResults = (resolvedMetadata as ResolvedProjectStatistics)?.OperationResults ?? new List<OperationResult>(),
                TargetFramework = resolvedMetadata.TargetFramework,
                RunDotvvmCompiler = resolvedMetadata.RunDotvvmCompiler,
                AssemblyName = resolvedMetadata.AssemblyName
            };
        }
        public static bool HasDotvvmFramework(this IResolvedProjectMetadata metadata)
        {
            return File.Exists(Path.Combine(GetWebsiteRootPath(metadata), Constants.BuildPath, Constants.DotvvmFrameworkAssembly));
        }

        public static string GetWebsiteRootPath(this IResolvedProjectMetadata metadata)
        {
            return Path.GetDirectoryName(metadata.CsprojFullName);
        }

        public static string GetWebsiteAssemblyPath(this IResolvedProjectMetadata metadata)
        {
            var defaultPath = GetWebsiteAssemblyPath(metadata, ".dll");
            if (File.Exists(defaultPath)) return defaultPath;
            var secondaryPath = GetWebsiteAssemblyPath(metadata, ".exe");
            return File.Exists(defaultPath) ? secondaryPath : defaultPath;
        }

        public static string GetWebsiteAssemblyPath(this IResolvedProjectMetadata metadata, string extenstion)
        {
            return Path.Combine(GetWebsiteRootPath(metadata), Constants.BuildPath,
                metadata.AssemblyName + extenstion);
        }

        public static void VerifyWebsiteAssemblyExistence(this IResolvedProjectMetadata metadata)
        {
            var assemblyPath = GetWebsiteAssemblyPath(metadata);
            if (!File.Exists(assemblyPath)) throw new FileNotFoundException("Dll was not found.", assemblyPath);
        }
    }
}
