using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;

namespace DotVVM.CommandLine.ProjectSystem
{
    public static class ProjectUtils
    {
        public const string AssemblyNameProperty = "AssemblyName";
        public const string ResolveReferencesTarget = "ResolveReferences";
        public const string ResolveProjectReferencesTarget = "ResolveProjectReferences";
        public const string ProjectReferenceItemType = "ProjectReference";

        public static IResolvedProjectMetadata ResolveMetadata(string projectDirectory)
        {
            var csproj = new DirectoryInfo(projectDirectory).GetFiles("*.csproj").Single();
            var project = new Project(csproj.FullName);
            var projectInstance = BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(project);
            {
                var request = new BuildRequestData(projectInstance, new[] { ResolveProjectReferencesTarget });
                var result = BuildManager.DefaultBuildManager.Build(null, request);
            }
            {
                var request = new BuildRequestData(projectInstance, new[] { ResolveReferencesTarget });
                var result = BuildManager.DefaultBuildManager.Build(null, request);
            }
            var assemblyName = projectInstance.GetProperty(AssemblyNameProperty)?.EvaluatedValue;
            return new ResolvedProjectMetadata {
                AssemblyName = assemblyName,
                AssemblyPath = null, // TODO
                CsprojFullName = csproj.FullName,
                CsprojVersion = default, // TODO: Maybe use UsingMicrosoftNETSdk or NETCoreSdkVersion
                DotvvmPackageNugetFolders = null, // TODO
                DotvvmProjectDependencies = null,
                PackageNugetFolders = null, // TODO
                ProjectRootDirectory = projectDirectory, // TODO
                RunDotvvmCompiler = false, // TODO
                TargetFramework = default // TODO
            };
        }
    }
}
