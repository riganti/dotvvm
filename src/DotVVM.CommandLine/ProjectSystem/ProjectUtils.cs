using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;

namespace DotVVM.CommandLine.ProjectSystem
{
    public static class ProjectUtils
    {
        public static IResolvedProjectMetadata ResolveMetadata(string projectDirectory)
        {
            // load the project with MSBuild
            var csproj = new DirectoryInfo(projectDirectory).GetFiles("*.csproj").Single();
            var project = new Project(csproj.FullName);var names = Enum.GetNames(typeof(TargetFramework));
            var projectInstance = BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(project);

            var csprojVersion = GetCsprojVersion(projectInstance);
            var targetFramework = GetTargetFramework(projectInstance, csprojVersion);

            var assemblyName = projectInstance.GetPropertyValue("AssemblyName");
            bool shouldRunDotvvmCompiler = ShouldRunDotvvmCompiler(projectInstance, csprojVersion);
            var assemblyPath = GetAssemblyPath(projectDirectory, assemblyName, targetFramework, csprojVersion);
            var dotvvmDependencies = GetDotvvmDependencies(projectInstance);
            var packageDirectory = GetNuGetPackageDirectory(projectInstance, csprojVersion);
            var dotvvmPackage = dotvvmDependencies.SingleOrDefault(d =>
                d.Name.Equals("DotVVM", StringComparison.OrdinalIgnoreCase) && !d.IsProjectReference);
            var dotvvmPackageDirectory = dotvvmPackage != null
                 ? Path.Combine(packageDirectory, dotvvmPackage.Name, dotvvmPackage.Version)
                 : "";

            return new ResolvedProjectMetadata {
                CsprojVersion = csprojVersion,
                TargetFramework = targetFramework,
                CsprojFullName = csproj.FullName,
                AssemblyName = assemblyName,
                RunDotvvmCompiler = shouldRunDotvvmCompiler,
                AssemblyPath = assemblyPath,
                ProjectRootDirectory = projectDirectory,
                DotvvmProjectDependencies = dotvvmDependencies,
                PackageNugetFolders = new List<string> { packageDirectory },
                DotvvmPackageNugetFolders = new List<string> { dotvvmPackageDirectory }
            };
        }

        private static CsprojVersion GetCsprojVersion(ProjectInstance project)
        {
            return string.IsNullOrEmpty(project.GetPropertyValue("UsingMicrosoftNETSdk"))
                ? CsprojVersion.OlderProjectSystem
                : CsprojVersion.DotNetSdk;
        }

        private static TargetFramework GetTargetFramework(ProjectInstance project, CsprojVersion csprojVersion)
        {
            string targetName = "GetTargetFrameworks";
            var targetFrameworks = BuildManager.DefaultBuildManager.Build(null,
                new BuildRequestData(project, new[] { targetName }))[targetName].Items.Single()
                .GetMetadata("TargetFrameworks");
            if (string.IsNullOrEmpty(targetFrameworks)) {
                if (csprojVersion == CsprojVersion.DotNetSdk)
                    return TargetFramework.NetStandard;

                string frameworkVersion = project.GetPropertyValue("TargetFrameworkVersion");
                var match = Regex.Match(frameworkVersion, @"^v(\d).(\d).(\d)$");
                if (!match.Success)
                    return TargetFramework.NetFramework;

                StringBuilder sb = new StringBuilder("net");
                sb.Append(match.Groups[1].Value);
                sb.Append(match.Groups[2].Value);
                if (match.Groups[3].Value != "0")
                    sb.Append(match.Groups[3].Value);
                targetFrameworks = sb.ToString();
            }
            var names = Enum.GetNames(typeof(TargetFramework));
            return targetFrameworks.Split(';')
                .Select(s => s.Replace(".", "").Trim())
                .Select(s => names.FirstOrDefault(b => b.Equals(s, StringComparison.OrdinalIgnoreCase)))
                .Where(n => n is object)
                .Select(n => Enum.TryParse<TargetFramework>(n, out var t) ? t : TargetFramework.Unknown)
                .Aggregate((l, r) => l | r);
        }

        private static bool ShouldRunDotvvmCompiler(ProjectInstance project, CsprojVersion csprojVersion)
        {
            switch (csprojVersion)
            {
                case CsprojVersion.DotNetSdk:
                    // TODO: When is .net core project compatible?
                    return false;
                case CsprojVersion.OlderProjectSystem:
                    return project.GetPropertyValue("ProjectTypeGuids")
                        .Split(';')
                        .Contains(Constants.DotvvmProjectGuid);
                default:
                    return false;
            }
        }

        private static List<ProjectDependency> GetDotvvmDependencies(ProjectInstance project)
        {
            bool IsDotvvmReference(ProjectItemInstance item)
            {
                return item.EvaluatedInclude
                    .IndexOf("DotVVM", StringComparison.OrdinalIgnoreCase) != -1;
            }

            var dotvvmDependencies = new List<ProjectDependency>();
            var packages = project.GetItems("PackageReference")
                .Where(p => IsDotvvmReference(p))
                .Select(p => new ProjectDependency {
                    Name = p.GetMetadataValue("Include"),
                    Version = p.GetMetadataValue("Version")
                });
            dotvvmDependencies.AddRange(dotvvmDependencies);
            var projects = project.GetItems("ProjectReference")
                .Where(p => IsDotvvmReference(p))
                .Select(p => new ProjectDependency {
                    IsProjectReference = true,
                    ProjectPath = p.EvaluatedInclude,
                    Name = GetDotvvmProjectReferenceName(p.GetMetadataValue("Name"))
                });
            dotvvmDependencies.AddRange(projects);
            var references = project.GetItems("Reference")
                .Where(p => IsDotvvmReference(p))
                .Select(p => {
                    var assemblyName = new AssemblyName(p.EvaluatedInclude);
                    return new ProjectDependency {
                        Name = assemblyName.Name,
                        Version = assemblyName.Version.ToString()
                    };
                });
            dotvvmDependencies.AddRange(references);
            return dotvvmDependencies;
        }

        private static string GetDotvvmProjectReferenceName(string name)
        {
            switch (name) {
                case "DotVVM.Framework":
                    return "DotVVM";
                case "DotVVM.Framework.Hosting.Owin":
                    return "DotVVM.Owin";
                case "DotVVM.Framework.Hosting.AspNetCore":
                    return "DotVVM.AspNetCore";
                default:
                    return name;
            }
        }

        private static string GetNuGetPackageDirectory(ProjectInstance project, CsprojVersion csprojVersion)
        {
            switch(csprojVersion)
            {
                case CsprojVersion.OlderProjectSystem:
                    return Path.Combine(project.Directory, "..\\packages");
                case CsprojVersion.DotNetSdk:
                    return project.GetPropertyValue("NuGetPackageRoot");
                default:
                    return string.Empty;
            }
        }

        private static string GetAssemblyPath(string directory,
            string assemblyName,
            TargetFramework target,
            CsprojVersion csprojVersion)
        {
            var bin = Path.Combine(directory, "bin");
            var dlls = Directory.GetFiles(bin, assemblyName + ".dll", SearchOption.AllDirectories);
            if (dlls.Length != 0)
                return dlls[0];

            var exes = Directory.GetFiles(bin, assemblyName + ".dll", SearchOption.AllDirectories);
            if (exes.Length != 0)
                return exes[0];

            return null;
        }
    }
}
