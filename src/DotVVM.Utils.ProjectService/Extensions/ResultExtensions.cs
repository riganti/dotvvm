using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Utils.ConfigurationHost.Lookup;
using DotVVM.Utils.ConfigurationHost.Operations;
using DotVVM.Utils.ConfigurationHost.Output.Statistics;

namespace DotVVM.Utils.ConfigurationHost.Extensions
{
    public static class ResultExtensions
    {
        public static StatisticsResult ToStatisticsResult(this IResult searchResult)
        {
            return new StatisticsResult()
            {
                CsprojFullName = searchResult.CsprojFullName,
                CsprojVersion = searchResult.CsprojVersion,
                DotvvmPackagesVersions = searchResult.DotvvmPackagesVersions,
                OperationResults = (searchResult as StatisticsResult)?.OperationResults ?? new List<OperationResult>(),
                TargetFramework = searchResult.TargetFramework,
                RunDotvvmCompiler = searchResult.RunDotvvmCompiler,
                AssemblyName = searchResult.AssemblyName
            };
        }
        public static bool HasDotvvmFramework(this IResult result)
        {
            return File.Exists(Path.Combine(GetWebsiteRootPath(result), Constants.BuildPath, Constants.DotvvmFrameworkAssembly));
        }

        public static string GetWebsiteRootPath(this IResult result)
        {
            return Path.GetDirectoryName(result.CsprojFullName);
        }

        public static string GetWebsiteAssemblyPath(this IResult result)
        {
            var defaultPath = GetWebsiteAssemblyPath(result, ".dll");
            if (File.Exists(defaultPath)) return defaultPath;
            var secondaryPath = GetWebsiteAssemblyPath(result, ".exe");
            return File.Exists(defaultPath) ? secondaryPath : defaultPath;
        }

        public static string GetWebsiteAssemblyPath(this IResult result, string extenstion)
        {
            return Path.Combine(GetWebsiteRootPath(result), Constants.BuildPath,
                result.AssemblyName + extenstion);
        }

        public static void VerifyWebsiteAssemblyExistence(this IResult result)
        {
            var assemblyPath = GetWebsiteAssemblyPath(result);
            if (!File.Exists(assemblyPath)) throw new FileNotFoundException("Dll was not found.", assemblyPath);
        }
    }
}
